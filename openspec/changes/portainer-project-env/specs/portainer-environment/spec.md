# Delta: portainer-environment

## ADDED Requirements

### Requirement: PortainerEnvironment 实体
系统 SHALL 支持 `PortainerEnvironment` 实体：DataSourceId、EndpointId、Name、Type、HostIds、LastSyncAt。每个 Portainer 数据源可拥有多个 Environment（对应 Portainer Endpoint），支持 docker / agent 等不同接入方式。

#### Scenario: 自动同步所有 Environment
- **WHEN** 管理员进入 `/Admin/Sources`，或点击某数据源的「同步」
- **THEN** 系统调用 Portainer `/api/endpoints` 拉取全部 Endpoint
- **THEN** 按 `DataSourceId + EndpointId` upsert：新增缺失记录，更新已有记录的 Name/Type/LastSyncAt

#### Scenario: 同步失败不阻断页面
- **WHEN** Portainer 不可达或凭据无效
- **THEN** 页面正常渲染，并以提示条展示同步失败原因

#### Scenario: Endpoint 被移除
- **WHEN** Portainer 上某 Endpoint 已不存在
- **THEN** 对应 Environment 记录保留不自动删除，避免已导入应用失去归属

### Requirement: Environment Host 绑定
Environment SHALL 支持绑定零条或多条 Host（`HostIds`），作为导入 Host 候选池的约束。

#### Scenario: 绑定 Host
- **WHEN** 管理员在数据源页展开 Environment 并勾选 Host 后保存
- **THEN** Environment 保存所选 Host 的 Id 列表

#### Scenario: 未绑定
- **WHEN** Environment 的 `HostIds` 为空
- **THEN** 导入时回落展示全局 Host 目录

#### Scenario: 绑定已失效
- **WHEN** `HostIds` 含已删除的 Host
- **THEN** 导入候选池过滤悬空 Id；过滤后为空则视同未绑定并提示绑定已失效

### Requirement: Sources 页面内嵌 Environment 管理
`/Admin/Sources` 的 Portainer 数据源行 SHALL 支持展开显示其 Environment 列表，并在展开区提供 Host 绑定、同步与导入入口。

#### Scenario: 展开查看
- **WHEN** 点击数据源行「展开」
- **THEN** 显示该数据源下全部 Environment（名称、Type、EndpointId、Host 绑定状态）

#### Scenario: 从 Environment 进入导入
- **WHEN** 在展开区点击某 Environment 的「导入」
- **THEN** 打开 `/Admin/Import/{sourceId}/{environmentId}` 的 compose project 候选列表
