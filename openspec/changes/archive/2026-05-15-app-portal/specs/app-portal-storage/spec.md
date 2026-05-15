## ADDED Requirements

### Requirement: 数据模型定义
系统 SHALL 使用以下数据模型存储应用信息：

```
AppEntry:
  - Id: ObjectId            (主键，LiteDB 自动生成)
  - Name: string             (应用名称，必填)
  - Description: string?     (描述)
  - IconUrl: string?         (图标 URL)
  - IsEnabled: bool          (默认 true)
  - SortOrder: int           (排序值，升序)
  - Urls: List<AppUrl>       (访问地址列表)
  - CreatedAt: DateTime
  - UpdatedAt: DateTime

AppUrl:
  - Url: string              (完整 HTTP URL，必填)
  - DisplayName: string?     (地址显示名称)
```

#### Scenario: 新应用创建时自动设置时间戳
- **WHEN** 创建新应用
- **THEN** `CreatedAt` 和 `UpdatedAt` 自动设为当前 UTC 时间

#### Scenario: 更新应用时更新时间戳
- **WHEN** 更新已有应用
- **THEN** `UpdatedAt` 自动更新为当前 UTC 时间

### Requirement: LiteDB 服务注册
系统 SHALL 将 `LiteDatabase` 注册为 Singleton 服务，`AppService` 注册为 Scoped 服务。

#### Scenario: 依赖注入可用
- **WHEN** PageModel 构造函数请求 `IAppService`
- **THEN** DI 容器解析出已注册的 AppService 实例

### Requirement: CRUD 服务接口
系统 SHALL 通过 `IAppService` 接口提供以下数据操作方法：

| 方法 | 功能 |
|------|------|
| `GetAllAppsAsync()` | 获取全部应用（含禁用），按 SortOrder 排序 |
| `GetEnabledAppsAsync()` | 仅获取已启用应用，按 SortOrder 排序 |
| `GetByIdAsync(ObjectId id)` | 根据 ID 获取单个应用 |
| `CreateAsync(AppEntry app)` | 创建新应用 |
| `UpdateAsync(AppEntry app)` | 更新应用 |
| `DeleteAsync(ObjectId id)` | 删除应用 |
| `ToggleEnabledAsync(ObjectId id)` | 切换应用启用/禁用状态 |

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

### Requirement: 数据库路径可配置
数据库文件路径 SHALL 通过 `appsettings.json` 配置，若未配置则 fallback 到 `AppPortal.db`。

#### Scenario: 从 appsettings.json 读取路径
- **WHEN** `appsettings.json` 中配置了 `"LiteDb:Path": "Data/portal.db"`
- **THEN** 数据库文件创建在 `Data/portal.db`

#### Scenario: 未配置时使用默认路径
- **WHEN** `appsettings.json` 中未配置 LiteDb 路径
- **THEN** 数据库文件创建在运行目录的 `AppPortal.db`
