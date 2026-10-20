## MODIFIED Requirements

### Requirement: 数据模型定义
系统 SHALL 使用以下数据模型存储应用与基础设施信息：

```
Host:
  - Id: ObjectId
  - Name: string
  - Addresses: List<string>   (多个 IP/域名)
  - IsDefault: bool
  - SortOrder: int

AppEntry:
  - Id / Name / Description / IconUrl / IsEnabled / SortOrder
  - Modules: List<AppModule>
  - SourceId / ExternalId / SyncHint
  - CreatedAt / UpdatedAt

AppModule:
  - Name / Description / IsEnabled / Order / SourceService?
  - Addresses: List<string>   (完整 URL 字符串，scheme 必含）

DataSource:
  - Id: ObjectId              (LiteDB _id，外键/路由仍用 ObjectId)
  - Name: string              (全局唯一键，同时作为展示名；创建后不可修改)
  - Type / Config / IsEnabled / LastSyncAt / LastStatus
```

`DataSource.Name` SHALL 具有全局唯一索引；`Id` 仍为 ObjectId 主键，`AppEntry.SourceId` 继续引用 ObjectId。

#### Scenario: 模块地址为 URL 字符串
- **WHEN** 保存应用
- **THEN** 每个模块的 Addresses 为 `http://` 或 `https://` 开头的 URL 字符串列表

#### Scenario: 旧 Urls 惰性迁移
- **WHEN** 读取仅含旧 `Urls` 的 AppEntry
- **THEN** 迁为默认模块且 Addresses 为原 Url 字符串

#### Scenario: 时间戳
- **WHEN** 创建或更新应用
- **THEN** `CreatedAt`/`UpdatedAt` 自动维护（UTC）

#### Scenario: DataSource Name 唯一索引
- **WHEN** 初始化 `datasources` 集合
- **THEN** 为 `Name` 建立唯一索引
- **THEN** `Id` 仍为 ObjectId，`AppEntry.SourceId` 不改为 Name

### Requirement: CRUD 服务接口
系统 SHALL 通过 `IAppService` 等接口提供应用、Host、数据源的增删改查。

| 方法 | 功能 |
|------|------|
| `GetAllAppsAsync()` | 获取全部应用（含禁用），按 SortOrder 排序 |
| `GetEnabledAppsAsync()` | 仅获取已启用应用 |
| `CreateAsync` / `UpdateAsync` / `DeleteAsync` | 应用增改删 |
| `ToggleEnabledAsync` | 切换启用/禁用 |
| `IHostService` / `IDataSourceService` | Host 与数据源 CRUD + 测试连接；数据源创建时校验 Name 唯一与字符集 |
| `IImportService` | 候选列表、Draft 解析、幂等导入 |

#### Scenario: 删除不存在的应用
- **WHEN** 调用 `DeleteAsync` 传入不存在的 ID
- **THEN** 无操作，不抛异常

#### Scenario: 创建数据源校验 Name
- **WHEN** 调用数据源 `CreateAsync` 传入非法或重名 Name
- **THEN** 拒绝创建并返回可展示的错误原因
