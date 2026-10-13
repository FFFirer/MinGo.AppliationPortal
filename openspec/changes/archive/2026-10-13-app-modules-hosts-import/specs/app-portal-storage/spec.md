## MODIFIED Requirements

### Requirement: 数据模型定义
系统 SHALL 使用以下数据模型存储应用与基础设施信息：

```
Host:
  - Id: ObjectId
  - Name: string             (必填)
  - Address: string          (必填，IP 或域名)
  - SchemeDefault: string    (http|https，默认 http)
  - IsDefault: bool
  - SortOrder: int

AppEntry:
  - Id: ObjectId
  - Name: string             (必填)
  - Description: string?
  - IconUrl: string?
  - IsEnabled: bool          (默认 true)
  - SortOrder: int
  - Modules: List<AppModule>
  - HostRefs: List<HostRef>  ({HostId, IsPreferred})
  - SourceId: ObjectId?
  - ExternalId: string?
  - SyncHint: string?
  - CreatedAt / UpdatedAt: DateTime

AppModule:
  - Name: string
  - Description: string?
  - IsEnabled: bool          (默认 true)
  - Order: int
  - SourceService: string?
  - Endpoints: List<AppEndpoint>

AppEndpoint:
  - DisplayName: string?
  - Kind: Http|Tcp|Udp       (默认 Http)
  - HostId: ObjectId?
  - Port: int?
  - ContainerPort: int?
  - Path: string?
  - SchemeOverride: string?
  - UrlOverride: string?

DataSource:
  - Id: ObjectId
  - Name: string
  - Type: Portainer|ComposeFile|Manual
  - Config: string           (JSON)
  - IsEnabled: bool
  - LastSyncAt: DateTime?
```

#### Scenario: 新应用创建时自动设置时间戳
- **WHEN** 创建新应用
- **THEN** `CreatedAt` 和 `UpdatedAt` 自动设为当前 UTC 时间

#### Scenario: 更新应用时更新时间戳
- **WHEN** 更新已有应用
- **THEN** `UpdatedAt` 自动更新为当前 UTC 时间

#### Scenario: 旧 Urls 惰性迁移
- **WHEN** 读取到仅含旧 `Urls` 而无 `Modules` 的 AppEntry
- **THEN** 将 Urls 转为默认模块「访问入口」下的 Http Endpoint（UrlOverride=原 Url），并可回写

### Requirement: CRUD 服务接口
系统 SHALL 通过 `IAppService` 接口提供应用数据操作，并扩展 Host/DataSource 服务。

| 方法 | 功能 |
|------|------|
| `GetAllAppsAsync()` | 获取全部应用（含禁用），按 SortOrder 排序 |
| `GetEnabledAppsAsync()` | 仅获取已启用应用，按 SortOrder 排序 |
| `GetByIdAsync(ObjectId id)` | 根据 ID 获取单个应用 |
| `CreateAsync(AppEntry app)` | 创建新应用 |
| `UpdateAsync(AppEntry app)` | 更新应用 |
| `DeleteAsync(ObjectId id)` | 删除应用 |
| `ToggleEnabledAsync(ObjectId id)` | 切换应用启用/禁用状态 |

另 SHALL 提供 `IHostService`（Host CRUD）与 `IDataSourceService`（数据源 CRUD、测试连接）。

#### Scenario: 创建应用
- **WHEN** 调用 `CreateAsync` 传入有效的 AppEntry
- **THEN** 应用被插入到 `apps` 集合
- **THEN** 插入后 Id 字段被自动赋值

#### Scenario: 切换启用状态
- **WHEN** 调用 `ToggleEnabledAsync` 传入应用 ID
- **THEN** 该应用的 `IsEnabled` 值取反
- **THEN** `UpdatedAt` 更新

#### Scenario: 删除不存在的应用
- **WHEN** 调用 `DeleteAsync` 传入不存在的 ID
- **THEN** 无操作，不抛异常

#### Scenario: Host 列表可查询
- **WHEN** 调用 Host 服务获取全部 Host
- **THEN** 按 SortOrder 排序返回
