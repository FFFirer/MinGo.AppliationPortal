# Proposal: portainer-project-env

## Why

当前导入以 Portainer Stack 为维度（`/api/stacks` → stack 数字 Id），存在两个问题：

1. **无法发现原生 compose 项目**：通过 `docker compose up` 直接部署的项目不在 Portainer stacks 列表里，但容器带 `com.docker.compose.project` 标签，理应可被发现和导入
2. **Host 绑定粒度太粗**：Host 绑在 DataSource 上，一个 Portainer 实例只能绑一组 Host；实际场景中一个 Portainer 管理多个 Environment（Endpoint），每个 Environment 对应不同的 Agent/主机群，需要独立绑定

同时，应用管理需要支持直接删除以清理重复应用（同一 compose 项目在不同 Environment 重复导入等场景），当前必须先禁用再删除的流程阻碍了批量清理效率。

## What Changes

- **导入维度从 Stack 改为 Compose Project**：不再调用 `/api/stacks`，改为查询指定 Environment 下所有容器，按 `com.docker.compose.project` label 分组，统一发现 Portainer 管理的 stack 和原生 compose 部署
- **新增 PortainerEnvironment 实体**：位于 DataSource 与 Host 之间，自动从 Portainer `/api/endpoints` 同步；Host 绑定从 DataSource 下沉到 Environment
- **AppEntry 新增 EnvironmentId**：幂等匹配键从 `(SourceId, ExternalId)` 改为 `(SourceId, EnvironmentId, ExternalId)`；`ExternalId` 语义从 stack 数字 Id 改为 compose project name
- **应用管理支持直接删除**：去掉"仅已禁用应用可删除"的前置条件，任何应用均可直接删除；禁用/启用开关保留
- **存量数据自动迁移**：为每个已有 DataSource 创建默认 Environment；通过 Portainer API 将旧 stack Id 映射到 project name；DataSource.HostIds 搬到默认 Environment

## Capabilities

### New Capabilities

- `portainer-environment`: PortainerEnvironment 实体、自动同步、Host 绑定、Sources 页面展开区域管理

### Modified Capabilities

- `app-import-sources`: 导入维度从 stack 改为 compose project；导入路径增加 environment 维度；ExternalId 语义变更
- `app-portal-storage`: 新增 PortainerEnvironment 集合；DataSource 移除 HostIds/LastSyncAt/LastStatus；AppEntry 新增 EnvironmentId
- `app-portal-admin`: 应用删除去掉禁用前置条件

## Impact

- `AppPortal/Models/PortainerEnvironment.cs` — 新增实体
- `AppPortal/Models/DataSource.cs` — 移除 HostIds/LastSyncAt/LastStatus
- `AppPortal/Models/AppEntry.cs` — 新增 EnvironmentId
- `AppPortal/Services/IPortainerClient.cs` / `PortainerClient.cs` — 新增 endpoints/projects API；保留 stacks API 底层封装（供迁移使用），业务层不再调用
- `AppPortal/Services/IEnvironmentService.cs` / `EnvironmentService.cs` — 新增 CRUD + 同步
- `AppPortal/Services/DataSourceService.cs` — 移除 Host 绑定逻辑
- `AppPortal/Services/IAppSource.cs` / `PortainerAppSource.cs` — 适配 project 维度 + environment
- `AppPortal/Services/ImportService.cs` — 匹配键增加 EnvironmentId
- `AppPortal/Services/IAppService.cs` / `AppService.cs` — 删除去掉 IsEnabled 前置检查
- `AppPortal/Pages/Admin/Sources.cshtml(.cs)` — 嵌入 Environment 展开区域，移除 DataSource 级 Host 绑定
- `AppPortal/Pages/Admin/Import.cshtml(.cs)` — 路由增加 environmentId 参数
- `AppPortal/Pages/Admin/Delete.cshtml(.cs)` — 移除 IsEnabled 前置条件
- `AppPortal/Pages/Admin/Index.cshtml(.cs)` — 删除按钮不再受 IsEnabled 限制
- `Program.cs` — 注册 IEnvironmentService
- 存量迁移逻辑（启动时自动执行）
