## Context

MinGo AppliationPortal 是一个 .NET 10 ASP.NET Core Razor Pages 应用，使用 LiteDB 嵌入式数据库和 Tailwind CSS v4 样式框架。当前仅支持通过 `dotnet run` 在开发环境中直接运行，缺少标准化容器镜像和自动化 CI/CD 流水线。

**当前状态：**
- 单项目结构（无 .sln 文件），`AppPortal/` 为唯一项目
- 构建流程：`npm ci → npx tailwindcss → dotnet publish`
- 数据库：LiteDB 文件数据库，路径通过 `appsettings.json` 配置 (`LiteDb:Path`)
- 无 Dockerfile、无 CI/CD 配置、无容器化部署方案

**约束：**
- 私有 Container Registry 用于镜像存储
- 打 Git Tag (v*.*.*) 或手动触发 CI
- 镜像标签策略：latest + semver (version, major.minor, major)
- Docker Compose 部署，LiteDB 通过 volume 持久化
- CI 容器环境无 Node.js，Tailwind CSS 需在 CI workflow 中预编译

## Goals / Non-Goals

**Goals:**
- 创建可复现的 Docker 多阶段构建流程，产出精简的 ASP.NET runtime 镜像
- 建立 Gitea Actions 自动化流水线，tag 推送/手动触发时自动构建并推送到私有 Registry
- 提供 Docker Compose 部署配置，含 LiteDB 数据持久化
- CI 构建缓存加速（registry cache），避免每次全量构建

**Non-Goals:**
- 不引入新的数据库（保持 LiteDB 嵌入式方案）
- 不在容器内处理 HTTPS（由反向代理层处理，如 Nginx/Caddy）
- 不涉及 Kubernetes 部署编排
- 不修改现有应用功能逻辑

## Decisions

### 1. 构建策略：CI 预编译 CSS + Docker SDK-only 构建

| 选项 | 方案 | 选择 |
|------|------|------|
| A | Dockerfile 内安装 Node.js 编译 Tailwind | ❌ 镜像体积大，增加维护复杂度 |
| B | CI 中预编译 CSS，Dockerfile 只含 SDK | ✅ **选定** |

**理由：** CI runner 天然有 Node.js 环境（`setup-node` action），预编译 CSS 后 Dockerfile 不需要 Node.js，最终镜像只含 ASP.NET runtime，更小更安全。

### 2. Docker 基础镜像：ASP.NET 10.0

使用 `mcr.microsoft.com/dotnet/aspnet:10.0` 作为 runtime 基础镜像，`mcr.microsoft.com/dotnet/sdk:10.0` 作为 build 阶段镜像。两者都是 .NET 官方镜像，版本与项目 target framework (net10.0) 严格对齐。

### 3. 容器端口：8080

Kestrel 监听 `http://+:8080`，这是一个约定俗成的"应用容器端口"标准。HTTPS 由外部反向代理终止，容器内不处理证书。

### 4. LiteDB 持久化：Docker Volume

LiteDB 是文件数据库，数据文件存储在容器的 `/data/AppPortal.db`。Docker Compose 定义命名 volume `app-portal-data` 挂载到 `/data`，确保容器重建不丢数据。

数据库路径通过环境变量 `LiteDb__Path=/data/AppPortal.db` 配置（双下划线是 .NET 配置系统对环境变量层级分隔符 `:` 的替代写法）。

### 5. Gitea Actions 触发策略

| 事件 | 行为 |
|------|------|
| `push: tags: v*.*.*` | 自动构建并推送镜像 |
| `workflow_dispatch` | 手动触发构建并推送镜像 |
| 普通 push | 不触发（避免无效构建） |

### 6. 镜像标签策略

使用 `docker/metadata-action@v5` 生成标签：

| 来源 | 生成的标签 |
|------|-----------|
| Tag `v1.2.3` | `1.2.3`, `1.2`, `1`, `latest` |
| Tag `v2.0.0` | `2.0.0`, `2.0`, `2`, `latest` |

`latest` 只在该次触发来自默认分支时添加（`enable={{is_default_branch}}`）。

### 7. 构建缓存：Registry Cache

使用 `type=registry` 缓存，将构建层缓存存储在私有 Registry 的 `:buildcache` 标签下。相比 local cache，registry 缓存在不同 runner 实例之间共享，效果好得多。

对于私有 Registry 如果使用 HTTP 协议，需要为 BuildX 配置 `insecure = true`。

### 8. csproj 修改策略

原 csproj 的 TailwindBuild target 无条件地在每次 Build 前执行 `npx tailwindcss`。当 Dockerfile 中构建时没有 Node.js，需要让它跳过。

修改方案：添加条件 `And !Exists('wwwroot/css/app.css')`，如果 CSS 文件已存在（CI 预编译过了），则跳过 Tailwind 构建。

## Risks / Trade-offs

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 私有 Registry 使用 HTTP 协议 | Docker BuildX 默认拒绝 HTTP registry | 在 `setup-buildx-action` 中配置 `config-inline` 允许 insecure registry |
| .NET 10 SDK 镜像尚未发布 (截至写此文时) | 构建镜像可能需要等待 | 如不可用，可暂时使用 `mcr.microsoft.com/dotnet/sdk:9.0`，csproj 改为 `net9.0`；或等 .NET 10 SDK 镜像可用 |
| CI runner 磁盘空间不足 | 大镜像构建可能失败 | 使用 registry cache 减少本地层存储；定期清理 runner 磁盘 |
| LiteDB 文件锁定 | 多个容器实例同时访问同一 volume 可能冲突 | LiteDB 是嵌入式数据库，设计为单进程访问；Docker Compose 确保 app-portal 是单实例 |
| Gitea Actions Runner 不支持 DinD | Docker build/push 会失败 | 确保 runner 使用 `catthehacker/ubuntu:act-latest` 容器镜像（预装 Docker） |
