## ADDED Requirements

（无新增需求）

## MODIFIED Requirements

### Requirement: Docker 镜像构建与推送
工作流 SHALL 使用 Docker BuildX 构建多平台镜像并推送到私有 Registry。

#### Scenario: Docker BuildX 配置
- **WHEN** 工作流进入 Docker 构建阶段
- **THEN** 使用 `docker/setup-buildx-action@v3` 初始化 BuildX
- **THEN** 使用 `docker/setup-qemu-action@v3` 配置 QEMU（多平台支持）

#### Scenario: 登录私有 Registry
- **WHEN** 构建镜像前
- **THEN** 使用 `docker/login-action@v3` 登录私有 Registry
- **THEN** Registry 地址从 Gitea Variables 读取：`vars.DOCKER_REGISTRY`
- **THEN** 用户名从 Gitea Variables 读取：`vars.DOCKER_USERNAME`
- **THEN** 密码从 Gitea Secrets 读取：`secrets.DOCKER_PASSWORD`

#### Scenario: 构建并推送镜像
- **WHEN** 登录成功
- **THEN** 使用 `docker/build-push-action@v6` 构建并推送镜像
- **THEN** `push: true` 确保镜像推送到 Registry
- **THEN** 使用 `context: .` 使用项目根目录作为构建上下文
- **THEN** 前端构建（Tailwind CSS）在 Dockerfile 内部由 frontend-build 阶段处理

## REMOVED Requirements

### Requirement: Tailwind CSS 预编译
**Reason**: Tailwind CSS 构建已移入 Dockerfile 的 frontend-build 阶段，CI 不再需要独立的 Node.js 预编译步骤。
**Migration**: 移除 `.gitea/workflows/docker-publish.yml` 中的 `actions/setup-node`、`npm ci` 和 `npm run css:prod` 步骤。
