## Purpose

定义 MinGo AppliationPortal 的 Docker 容器镜像构建规范。基于三阶段构建，在 Dockerfile 内部编译 Tailwind CSS 后使用 .NET SDK 编译发布，最终产出精简的 ASP.NET runtime 镜像。

## Requirements

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

### Requirement: 容器内 Kestrel 配置
容器内的 Kestrel SHALL 监听 `http://+:8080`，不处理 HTTPS。

#### Scenario: 默认端口为 8080
- **WHEN** 容器启动
- **THEN** `ASPNETCORE_URLS` 环境变量设为 `http://+:8080`
- **THEN** 应用通过 `http://localhost:8080` 可访问

### Requirement: 环境变量配置数据库路径
LiteDB 数据库路径 SHALL 通过环境变量 `LiteDb__Path` 配置，默认值为 `/data/AppPortal.db`。

#### Scenario: 环境变量覆盖数据库路径
- **WHEN** 容器设置环境变量 `LiteDb__Path=/data/AppPortal.db`
- **THEN** LiteDB 在 `/data/` 目录创建 `AppPortal.db` 文件

### Requirement: 项目文件修改
`AppPortal.csproj` 中的 `TailwindBuild` Target SHALL 增加条件，当 `wwwroot/css/app.css` 已存在时跳过执行。

#### Scenario: CSS 文件已存在时跳过 Tailwind 构建
- **WHEN** CI 构建前已预编译了 CSS（`wwwroot/css/app.css` 存在）
- **THEN** `dotnet build` 跳过 TailwindBuild target
- **THEN** 构建成功不报错

#### Scenario: CSS 文件不存在时正常执行 Tailwind 构建
- **WHEN** 本地开发环境 `wwwroot/css/app.css` 不存在
- **THEN** `dotnet build` 正常执行 `npx tailwindcss` 编译 CSS
