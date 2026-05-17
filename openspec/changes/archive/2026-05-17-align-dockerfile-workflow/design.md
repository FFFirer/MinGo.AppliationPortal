## Context

当前 AppPortal 构建流程采用"CI 预编译 Tailwind CSS → Docker Build"的两步模式。ReverseProxy 项目使用"前端构建内置于 Dockerfile"的单步模式。本设计将 AppPortal 对齐到后者的架构：在 Dockerfile 中新增 frontend-build 阶段，让 CI 只负责 docker build & push。

## Goals / Non-Goals

**Goals:**
- Dockerfile 改为 3-stage：frontend-build → dotnet-build → runtime
- 移除 CI 中与 Node.js/Tailwind 相关的步骤（setup-node / npm ci / css:prod）
- Registry 凭证管理从 `secrets.REGISTRY_*` 迁移至 `vars.DOCKER_*` + `secrets.DOCKER_PASSWORD` 模式（对齐 ReverseProxy）
- 保留现有构建缓存（`:buildcache` tag）和 QEMU 多平台支持
- 保留 tag push 触发方式和 `docker/metadata-action` 版本标签策略
- 更新 OpenSpec specs 反映新设计

**Non-Goals:**
- 不改动 AppPortal.csproj（已有 `!Exists` 条件自动适配 CSS 文件存在时的行为）
- 不改动应用代码（Program.cs、Pages 等）
- 不引入新的外部依赖
- 不改变触发方式为 Release event

## Decisions

### Decision 1: Dockerfile 3-stage 架构

**方案**: 新增 `frontend-build` 阶段，使用 `node:22-alpine` 镜像安装 npm 依赖并编译 Tailwind CSS，生成的文件通过 `COPY --from=frontend-build` 注入 .NET build 阶段和最终 runtime 阶段。

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  frontend-build   │     │   backend-build   │     │     runtime       │
│  node:22-alpine   │     │  sdk:10.0         │     │  aspnet:10.0      │
│                   │     │                   │     │                  │
│  npm ci           │     │  dotnet restore   │     │  COPY --from=    │
│  npm run css:prod │────▶│  dotnet publish   │────▶│  /app/publish    │
│                   │     │                   │     │                  │
│  wwwroot/css/     │     │                   │     │  COPY --from=    │
│  app.css          │     │                   │     │  wwwroot/css/    │
└──────────────────┘     └──────────────────┘     └──────────────────┘
```

**理由**:
- 与 ReverseProxy 模式一致，降低跨项目认知成本
- CI 流程简化，只关注 Docker 构建
- 构建完全自包含，不依赖 CI 环境中的 Node.js

**权衡**: 每次 Docker 构建都会执行 `npm ci`，即使只有 .NET 代码变更也会触发；可通过 Docker layer caching（buildkit cache）缓解。

### Decision 2: CI 简化 — 移除前端构建步骤

当前 CI 有 4 个独立步骤处理 Tailwind CSS：
```
Setup Node.js → Install npm dependencies → Build Tailwind CSS → Docker build & push
```
改为单一入口：
```
Docker build & push (包含前端构建)
```

这会使 CI 流程缩减约 40% 的步骤数。

### Decision 3: Registry 凭证管理对齐

从当前：
```yaml
secrets.REGISTRY_URL
secrets.REGISTRY_USER
secrets.REGISTRY_PASSWORD
```
改为（对齐 ReverseProxy）：
```yaml
vars.DOCKER_REGISTRY
vars.DOCKER_USERNAME
secrets.DOCKER_PASSWORD
```

理由：Gitea 的 `vars` 适合非敏感配置（URL、用户名），`secrets` 仅用于密码。与 ReverseProxy 统一的命名便于跨项目管理。

### Decision 4: 构建缓存保留

虽然 ReverseProxy 无缓存配置，但当前项目已配置 registry build cache。保留该配置可加速 Frontend-build 阶段的 npm install 缓存命中，减少构建时间。

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|---------|
| [构建时间] Docker 内 npm install 每次执行，可能比 CI 缓存慢 | Docker layer caching + buildkit cache 可减少重复下载 |
| [镜像大小] frontend-build 阶段临时文件可能残留在最终镜像中 | 多阶段构建保证最终镜像只包含 COPIED 文件 |
| [调试] 前端构建失败时在 Docker 内排查不如 CI 直观 | 可在本地 `docker build --target frontend-build` 单独测试 |
| [Secret 迁移] `REGISTRY_*` → `DOCKER_*` 需同时更新 Gitea 仓库设置 | 需在 Gitea Web UI 中配置新的 vars 和 secrets |
