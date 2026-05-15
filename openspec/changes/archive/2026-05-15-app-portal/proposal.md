## Why

团队需要一个统一的应用门户站点，集中展示内部所有 Web 应用的入口和访问地址，避免成员记忆零散的 URL 或通过书签导航。同时需要一个管理后台来动态维护应用列表，支持启用/禁用控制，无需修改代码即可上下线应用。

## What Changes

- 新建 ASP.NET Core 10 Razor Pages 项目，集成 Tailwind CSS v4
- 首页展示所有已启用的应用卡片，每个应用可配置多个 HTTP 访问地址，点击在新标签页打开
- 管理后台支持应用列表的增删改查，支持启用/禁用切换
- 使用 LiteDB 嵌入式数据库存储应用数据，无需外部数据库依赖
- 数据库路径通过 appsettings.json 配置，支持 fallback 默认值

## Capabilities

### New Capabilities

- `app-portal-home`: 门户首页，以卡片网格展示所有已启用的应用，每个应用显示名称、描述、图标及多个访问地址链接，点击在新标签页跳转
- `app-portal-admin`: 管理后台，以表格展示全部应用（含已禁用的），支持新增、编辑、删除、启用/禁用切换操作，表单支持动态增减多个地址
- `app-portal-storage`: LiteDB 持久化层，定义 AppEntry 数据模型，提供完整 CRUD 服务接口，数据库路径可配置

### Modified Capabilities

*无 — 全新项目，无已有 spec 需要修改。*

## Impact

- 新项目 `AppPortal` 创建于 `MinGo.AppliationPortal` 仓库
- 新增 NuGet 依赖：`LiteDB`（嵌入式 NoSQL 数据库）
- 新增 NPM 依赖：`tailwindcss`、`@tailwindcss/cli`
- Tailwind CSS v4 构建步骤集成到 MSBuild 管线（.csproj Target）
- 数据库文件 `AppPortal.db` 默认生成在项目运行目录
