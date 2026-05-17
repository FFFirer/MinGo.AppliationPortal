## Why

当前 MinGo.AppliationPortal 的 Docker 镜像构建流程与 MinGo.ReverseProxy 项目存在不一致：ReverseProxy 使用 3-stage Dockerfile（前端构建内置于 Docker），而 AppPortal 将 Tailwind CSS 构建放在 CI 中独立执行后再交给 Docker。这种不一致增加了维护成本，且 CI 流程可简化。对齐两个项目的构建模式，降低认知负载。

## What Changes

1. **Dockerfile** — 从 2-stage 改为 3-stage，新增 `frontend-build` 阶段，将 Tailwind CSS 构建移入 Docker 内部，不再依赖 CI 预编译 CSS
2. **`.gitea/workflows/docker-publish.yml`** — 移除 CI 中的 `actions/setup-node` 和 Tailwind 构建步骤（因已移入 Dockerfile），Registry 配置改用 `vars.DOCKER_REGISTRY` + `secrets.DOCKER_PASSWORD` 模式，版本提取逻辑对齐 ReverseProxy
3. **`docker-compose.yml`** — 更新 `image` 字段命名模式对齐新构建流程
4. **现有 Specs** — 更新 `container-image/spec.md` 和 `gitea-cd/spec.md` 反映新的设计和流程

## Capabilities

### New Capabilities

- _无新增 capability_

### Modified Capabilities

- `container-image`: Dockerfile 改为 3-stage，将 Tailwind CSS 构建移入 frontend-build 阶段
- `gitea-cd`: CI 流程简化，移除 Node.js/Tailwind 预编译步骤，Registry 配置模式和版本提取方式变更
- `deployment-compose`: docker-compose.yml 镜像命名模式对齐新流程

## Impact

- **Dockerfile**: 新增 Node.js 构建阶段，镜像构建时间可能略增（每次构建需 npm install），但 CI 整体简化
- **CI Workflow**: 移除 setup-node + npm ci + css:prod 步骤，改为单一 docker build 入口
- **现有 Specs**: 需要更新 `container-image/spec.md` 和 `gitea-cd/spec.md` 中的需求描述
- **AppPortal.csproj**: 无需修改（已有 `!Exists` 条件自动适配）
