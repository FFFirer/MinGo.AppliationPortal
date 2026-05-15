## Why

MinGo AppliationPortal 目前仅支持本地开发运行，缺少标准化的容器镜像构建和自动化 CI/CD 流水线。这在部署到生产环境时需要手动操作，容易出错且不可重复。需要引入 Docker 容器化 + Gitea Actions 自动化流水线，实现从代码提交到镜像发布的标准化流程。

## What Changes

- **Dockerfile** — 新增多阶段构建文件，在 CI 中预编译 Tailwind CSS 后，使用 .NET SDK 编译发布并打包为精简 ASP.NET runtime 镜像
- **docker-compose.yml** — 新增 Docker Compose 编排文件，定义 app-portal 服务、端口映射、环境变量和 LiteDB 数据卷持久化
- **.gitea/workflows/docker-publish.yml** — 新增 Gitea Actions 工作流，监听 Git Tag (v*.*.*) 和手动触发 (workflow_dispatch)，自动构建 Docker 镜像并推送到私有 Registry
- **AppPortal.csproj** — 修改 TailwindBuild MSBuild Target，当 `wwwroot/css/app.css` 已存在时跳过执行（CI 环境无 Node.js 时不会报错）

## Capabilities

### New Capabilities
- `container-image`: 基于 Docker 多阶段构建的容器镜像，使用私有 Registry 分发，支持 semver 版本标签
- `gitea-cd`: Gitea Actions 工作流，打 tag 或手动触发时自动构建镜像并推送到私有 Registry，含 registry cache 加速
- `deployment-compose`: Docker Compose 部署配置，定义服务端口、环境变量和 LiteDB volume 数据持久化

### Modified Capabilities

<!-- 无现有能力需要修改 -->

## Impact

- **新增文件**: Dockerfile, docker-compose.yml, .gitea/workflows/docker-publish.yml
- **修改文件**: AppPortal/AppPortal.csproj（TailwindBuild 条件判断）
- **新增依赖**: 私有 Container Registry（需在 Gitea Secrets 中配置 REGISTRY_URL/USER/PASSWORD）
- **部署方式变更**: 从 dotnet run 开发模式转为 Docker Compose 容器化部署
- **数据持久化**: LiteDB 数据库文件通过 Docker volume 挂载到容器内 /data 目录
