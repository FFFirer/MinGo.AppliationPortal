# Proposal: datasource-name-unique-key

## Why

数据源 `Name` 目前只是自由文本展示字段，真正唯一键是 LiteDB `ObjectId`。构建 Portainer client 时所有数据源共用 HttpClient 名 `"portainer"`，多源并发下 `BaseAddress` / `DefaultRequestHeaders` 会互相踩踏。需要把 `Name` 升为可视化唯一键，并拼进 HttpClient 名，既隔离多源线路，又让「看到的名字」与「配置/客户端标识」三位一体。

## What Changes

- `DataSource.Name` 升为**全局唯一、创建后不可修改**的可视化唯一键（同时是展示名与 client 键）
- `Name` 仅允许 `[A-Za-z0-9_-]`，禁 `-`/`_` 开头，长度 1–64，大小写敏感
- 创建数据源时校验字符集/长度/唯一性；重名或非法字符拒绝创建
- 删除数据源后同名可复用
- `datasources` 集合为 `Name` 建唯一索引
- 构建 Portainer client 时按**类型前缀 + Name** 拼接 HttpClient 名（如 `portainer:prod-01`），各数据源独立 handler / BaseAddress / 认证头
- 不引入独立中文 Display 字段；`Name` 即展示名
- 不修改 `ObjectId` 主键、`AppEntry.SourceId` 外键、`/Admin/Import/{sourceId}` 路由

## Capabilities

### New Capabilities

（无）

### Modified Capabilities

- `app-import-sources`: 数据源 `Name` 的唯一性、字符集与不可改约束；创建入口校验；HttpClient 名按 `类型前缀:Name` 拼接
- `app-portal-storage`: `DataSource.Name` 唯一索引与主键关系说明（ObjectId 仍为 `_id`，Name 为业务唯一键）

## Impact

- `AppPortal/Models/DataSource.cs` — Name 语义注释/约束说明
- `AppPortal/Services/DataSourceService.cs` — 唯一索引、按 Name 查询、重名校验
- `AppPortal/Services/IDataSourceService.cs` — 可能新增 `GetByNameAsync`
- `AppPortal/Services/PortainerClient.cs` — `CreateClient` 按类型前缀+Name 拼接
- `AppPortal/Services/IPortainerClient.cs` — client 构建入参可能携带 source 名/类型
- `AppPortal/Services/PortainerAppSource.cs` / `ImportService.cs` — 调用侧传递唯一键
- `AppPortal/Pages/Admin/Sources.cshtml(.cs)` — 创建表单校验与错误提示；明确 Name 不可改
- 存量数据：若已有非法/重名 Name，新建校验只管新增；旧记录不强制迁移（空库则无影响）
