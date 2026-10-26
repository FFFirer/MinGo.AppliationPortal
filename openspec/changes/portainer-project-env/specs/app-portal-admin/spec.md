# Delta: app-portal-admin

## MODIFIED Requirements

### Requirement: 删除应用
任何状态的应用 SHALL 支持直接删除；删除前 SHALL 展示二次确认页，删除后不可恢复。

#### Scenario: 已启用应用可直接删除
- **WHEN** 在 `/Admin` 点击已启用应用的「删除」并确认
- **THEN** 应用被删除并返回管理列表

#### Scenario: 已禁用应用可删除
- **WHEN** 在 `/Admin` 点击已禁用应用的「删除」并确认
- **THEN** 应用被删除并返回管理列表

#### Scenario: 删除确认页
- **WHEN** 打开 `/Admin/Delete/{id}`
- **THEN** 展示应用摘要与不可恢复提示
- **THEN** 不因应用处于启用状态而拒绝打开

### Requirement: 数据源与导入入口
`/Admin/Sources` SHALL 列出数据源并支持创建 Portainer 源、测试连接、同步 Environment、删除；每个 Portainer 数据源行可展开查看其 Environment，并从 Environment 进入导入。

#### Scenario: 进入导入
- **WHEN** 在数据源展开区点击某 Environment 的「导入」
- **THEN** 打开 `/Admin/Import/{sourceId}/{environmentId}` 候选列表

#### Scenario: 兼容旧入口
- **WHEN** 访问仅含 sourceId 的 `/Admin/Import/{sourceId}`
- **THEN** 回落到该数据源的第一个 Environment
