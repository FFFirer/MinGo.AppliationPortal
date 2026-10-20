## MODIFIED Requirements

### Requirement: 数据源实体
系统 SHALL 支持 `DataSource` 实体：Name、Type(Portainer|ComposeFile|Manual)、Config(JSON)、IsEnabled、LastSyncAt、LastStatus。同一 Type 可有多条数据源（多个 Portainer 实例）。

`Name` SHALL 作为数据源的可视化唯一键（同时用于展示与 client 标识拼接），创建后不可修改。

#### Scenario: 创建 Portainer 数据源
- **WHEN** 管理员在 `/Admin/Sources` 创建 Type=Portainer 的数据源
- **THEN** 必填 Name、BaseUrl、ApiKey（或用户名密码）
- **THEN** 可执行「测试连接」并显示连通结果

#### Scenario: 测试连接失败
- **WHEN** Portainer BaseUrl 或 ApiKey 无效
- **THEN** 显示错误信息，数据源仍可保存但标记为连接失败

#### Scenario: Name 唯一且创建后不可改
- **WHEN** 创建数据源时 Name 与已有数据源（任意 Type）重名
- **THEN** 拒绝创建并提示名称已存在
- **THEN** 已创建的数据源不提供修改 Name 的入口

#### Scenario: 删除后同名可复用
- **WHEN** 删除某数据源后以同一 Name 创建新数据源
- **THEN** 创建成功

## ADDED Requirements

### Requirement: 数据源 Name 校验
数据源 `Name` SHALL 全局唯一（跨 Type），并满足：仅含大小写字母、数字、下划线、横杠（`[A-Za-z0-9_-]`）；不得以 `-` 或 `_` 开头；长度 1–64；大小写敏感。

#### Scenario: 合法 Name
- **WHEN** 创建 Name 为 `prod-01` 或 `Prod_01` 的数据源
- **THEN** 校验通过

#### Scenario: 非法字符
- **WHEN** 创建 Name 为 `生产 Portainer` 或 `prod.01` 或 `prod/01` 的数据源
- **THEN** 拒绝创建并提示 Name 仅允许大小写字母、数字、下划线、横杠

#### Scenario: 非法首字符或超长
- **WHEN** 创建 Name 以 `-` 或 `_` 开头，或长度不在 1–64
- **THEN** 拒绝创建并提示对应校验错误

#### Scenario: 大小写敏感
- **WHEN** 已存在 Name=`Prod` 的数据源，再创建 Name=`prod`
- **THEN** 创建成功（两者视为不同 Name）

### Requirement: Portainer client 唯一键拼接
构建 Portainer client 实例时，系统 SHALL 按「类型前缀 + Name」拼接 HttpClient 名（如 `portainer:{Name}`），使不同数据源的 BaseAddress 与认证配置相互隔离。

#### Scenario: 多数据源独立 client
- **WHEN** 存在 Name=`prod-01` 与 Name=`prod-02` 的两个 Portainer 数据源并发请求
- **THEN** 分别使用 `portainer:prod-01` 与 `portainer:prod-02` 的 HttpClient
- **THEN** 任一方的 BaseAddress/API Key 不覆盖另一方

#### Scenario: 类型前缀命名空间
- **WHEN** 构建 Type=Portainer 的 client
- **THEN** HttpClient 名为 `portainer:{Name}`
