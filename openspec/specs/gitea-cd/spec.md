## Purpose

定义 MinGo AppliationPortal 的 Gitea Actions 持续交付流水线规范。基于 Git Tag 和手动触发，自动构建 Docker 镜像并推送到私有 Registry。

## Requirements

### Requirement: Gitea Actions 工作流文件
项目根目录 SHALL 包含 `.gitea/workflows/docker-publish.yml`，定义 Docker 镜像的自动构建与发布流水线。

#### Scenario: 工作流定义
- **WHEN** 用户查看 `.gitea/workflows/docker-publish.yml`
- **THEN** 文件使用 YAML 格式定义 Gitea Actions 工作流
- **THEN** 工作流名称为 `Docker Build & Publish`

### Requirement: 触发条件
工作流 SHALL 在以下条件下触发：
- 推送匹配 `v*.*.*` 的 Git Tag 时自动触发
- 通过 Gitea Web UI 手动触发（`workflow_dispatch`）

#### Scenario: Tag 推送触发自动构建
- **WHEN** 用户推送 Git Tag `v1.2.3` 到仓库
- **THEN** 工作流自动启动

#### Scenario: 手动触发
- **WHEN** 用户在 Gitea Actions 页面点击 "Run workflow"
- **THEN** 工作流手动启动

#### Scenario: 普通 push 不触发
- **WHEN** 用户推送代码到任意分支（无 tag）
- **THEN** 工作流不启动

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

### Requirement: 镜像标签策略
工作流 SHALL 使用 `docker/metadata-action@v5` 生成镜像标签，遵循 latest + semver 策略。

#### Scenario: 语义版本标签
- **WHEN** 触发源自 Git Tag `v1.2.3`
- **THEN** 生成标签 `1.2.3`（完整版本）
- **THEN** 生成标签 `1.2`（major.minor）
- **THEN** 生成标签 `1`（major）
- **THEN** 生成标签 `latest`

#### Scenario: latest 标签条件
- **WHEN** 来自非默认分支的触发
- **THEN** `latest` 标签不被添加
- **WHEN** 来自默认分支的触发
- **THEN** `latest` 标签被添加

### Requirement: 构建缓存
工作流 SHALL 使用 Registry 缓存加速后续构建。

#### Scenario: Registry 缓存
- **WHEN** Docker 构建完成
- **THEN** 构建层缓存被推送到 `:buildcache` 标签
- **THEN** 缓存模式使用 `mode=max` 存储所有层
- **WHEN** 后续构建开始
- **THEN** 从 Registry 拉取 `:buildcache` 作为缓存源
