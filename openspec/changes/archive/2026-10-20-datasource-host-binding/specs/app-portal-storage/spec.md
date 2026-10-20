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
  - Addresses: List<string>   (完整 URL 字符串，scheme 必含)

DataSource:
  - Id / Name / Type / Config / IsEnabled / LastSyncAt / LastStatus
  - HostIds: List<ObjectId>   (绑定 Host；空 = 未绑定，导入回落全量)
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

#### Scenario: DataSource HostIds 缺省
- **WHEN** 读取无 `HostIds` 字段的存量 DataSource
- **THEN** 视为空列表（未绑定），不报错、不迁移
