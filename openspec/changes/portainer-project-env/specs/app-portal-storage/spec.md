# Delta: app-portal-storage

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

PortainerEnvironment:
  - Id / DataSourceId / EndpointId / Name / Type
  - HostIds: List<ObjectId>   (绑定 Host；空 = 未绑定，导入回落全量)
  - LastSyncAt: DateTime?

AppEntry:
  - Id / Name / Description / IconUrl / IsEnabled / SortOrder
  - Modules: List<AppModule>
  - SourceId / EnvironmentId / ExternalId / SyncHint
  - CreatedAt / UpdatedAt

AppModule:
  - Name / Description / IsEnabled / Order / SourceService?
  - Addresses: List<string>   (完整 URL 字符串，scheme 必含)

DataSource:
  - Id / Name / Type / Config / IsEnabled
```

#### Scenario: 模块地址为 URL 字符串
- **WHEN** 保存应用
- **THEN** 每个模块的 Addresses 为 `http://` 或 `https://` 开头的 URL 字符串列表

#### Scenario: 旧 Urls 惰性迁移
- **WHEN** 读取仅含旧 `Urls` 的 AppEntry
- **THEN** 迁为默认模块且 Addresses 为原 Url 字符串

#### Scenario: 时间戳
- **WHEN** 创建或更新应用
- **THEN** `CreatedAt`/`UpdatedAt` 自动维护（UTC）

#### Scenario: EnvironmentId 缺省
- **WHEN** 读取无 `EnvironmentId` 字段的存量 AppEntry
- **THEN** 视为 null（手工创建或待迁移），不报错

### Requirement: LiteDB 服务注册
系统 SHALL 将 `LiteDatabase` 注册为 Singleton 服务，`AppService`、`HostService`、`DataSourceService`、`EnvironmentService`、`ImportService`、`PortainerClient` 注册为 Scoped 服务。

#### Scenario: 依赖注入可用
- **WHEN** PageModel 构造函数请求上述服务
- **THEN** DI 容器解析出对应实例

### Requirement: CRUD 服务接口
系统 SHALL 通过 `IAppService` 等接口提供应用、Host、数据源、Environment 的增删改查。

| 方法 | 功能 |
|------|------|
| `GetAllAppsAsync()` | 获取全部应用（含禁用），按 SortOrder 排序 |
| `GetEnabledAppsAsync()` | 仅获取已启用应用 |
| `CreateAsync` / `UpdateAsync` / `DeleteAsync` | 应用增改删 |
| `ToggleEnabledAsync` | 切换启用/禁用 |
| `IHostService` / `IDataSourceService` | Host 与数据源 CRUD + 测试连接 |
| `IEnvironmentService` | Environment 查询、Host 绑定、Portainer 同步、删除 |
| `IImportService` | 候选列表、Draft 解析、幂等导入（含 EnvironmentId） |

#### Scenario: 删除不存在的应用
- **WHEN** 调用 `DeleteAsync` 传入不存在的 ID
- **THEN** 无操作，不抛异常

### Requirement: 存量数据自动迁移
系统 SHALL 在启动时执行一次性迁移（以 `meta` 集合标记），将旧数据映射到新模型；迁移失败不阻断启动，并保留标记以便下次重试。

#### Scenario: DataSource HostIds 迁移
- **WHEN** 存量 DataSource 含 `HostIds`
- **THEN** 复制到该数据源下所有 Environment 的 `HostIds`（原绑定语义保持）

#### Scenario: ExternalId 迁移
- **WHEN** 存量 AppEntry 的 `ExternalId` 为数字（旧 stack Id）且可查到对应 stack
- **THEN** `ExternalId` 更新为 stack 名称（compose project name）
- **THEN** `EnvironmentId` 指向该 stack 所属 Endpoint 对应的 Environment

#### Scenario: 迁移失败重试
- **WHEN** 某数据源迁移期间 Portainer 不可达
- **THEN** 记录日志且不写入完成标记，下次启动重试

### Requirement: 数据库路径可配置
数据库文件路径 SHALL 通过 `appsettings.json` 的 `LiteDb:Path` 配置，未配置时 fallback 到 `AppPortal.db`。

#### Scenario: 从 appsettings.json 读取路径
- **WHEN** 配置了 `"LiteDb:Path": "Data/portal.db"`
- **THEN** 数据库创建在 `Data/portal.db`
