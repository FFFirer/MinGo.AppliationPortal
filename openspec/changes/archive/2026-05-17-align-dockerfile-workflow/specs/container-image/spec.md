## ADDED Requirements

（无新增需求）

## MODIFIED Requirements

### Requirement: Dockerfile 实现多阶段构建
项目根目录 SHALL 包含一个 `Dockerfile`，实现多阶段构建，最终产出基于 `mcr.microsoft.com/dotnet/aspnet:10.0` 的精简运行时镜像。

#### Scenario: 三阶段构建流程
- **WHEN** 执行 `docker build`
- **THEN** frontend-build 阶段使用 `node:22-alpine` 镜像执行 `npm ci` 和 `npm run css:prod` 编译 Tailwind CSS
- **THEN** build 阶段使用 `mcr.microsoft.com/dotnet/sdk:10.0` 镜像执行 `dotnet restore` 和 `dotnet publish -c Release`
- **THEN** final 阶段使用 `mcr.microsoft.com/dotnet/aspnet:10.0` 镜像，仅复制 publish 产物和前端构建产物
- **THEN** 最终镜像不包含 .NET SDK

#### Scenario: 前端构建产物流入
- **WHEN** frontend-build 阶段完成
- **THEN** `wwwroot/css/app.css` 被复制到 build 阶段
- **THEN** dotnet publish 自动包含该 CSS 文件到发布产物

## REMOVED Requirements

### Requirement: Dockerfile 不包含 Node.js/Tailwind 构建
**Reason**: Tailwind CSS 构建已移入 Dockerfile 的 frontend-build 阶段，不再由 CI 预编译。
**Migration**: Dockerfile 现在包含 Node.js 构建阶段，CI 不再需要 `actions/setup-node` 等步骤。
