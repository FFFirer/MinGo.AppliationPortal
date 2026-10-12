## ADDED Requirements

### Requirement: 数据源实体
系统 SHALL 支持 `DataSource` 实体：Name、Type(Portainer|ComposeFile|Manual)、Config(JSON)、IsEnabled、LastSyncAt。同一 Type 可有多条数据源（多个 Portainer 实例）。

#### Scenario: 创建 Portainer 数据源
- **WHEN** 管理员在 `/Admin/Sources` 创建 Type=Portainer 的数据源
- **THEN** 必填 Name、BaseUrl、ApiKey（或 Token）
- **THEN** 可执行「测试连接」并显示连通结果

#### Scenario: 测试连接失败
- **WHEN** Portainer BaseUrl 或 ApiKey 无效
- **THEN** 显示错误信息，数据源仍可保存但标记为连接失败

### Requirement: 导入管道与 AppDraft
系统 SHALL 通过 `IAppSource` 契约将外部元数据归一为 `AppDraft`，经预览确认后写入 `AppEntry`，并记录 `SourceId` 与 `ExternalId`。

#### Scenario: 列出候选
- **WHEN** 管理员选择某个 Portainer 数据源并进入导入页
- **THEN** 列出该实例下的 stacks（名称、状态、服务数）

#### Scenario: 预览 AppDraft
- **WHEN** 管理员选择某个 stack 进入预览
- **THEN** 展示建议的模块与入口（含端口映射），可勾选入口、修改模块名
- **THEN** 确认导入后创建或更新应用

#### Scenario: 再导入更新而非重复创建
- **WHEN** 对同一 SourceId+ExternalId 再次导入
- **THEN** 更新已有应用（模块/入口按预览结果写入），不新建第二条

### Requirement: Portainer stack 端口独立识别
从 Portainer 导入时，系统 SHALL 将每个 compose service 的端口仅落入该 service 对应模块，不得混入其他模块。

#### Scenario: 多 service 独立端口
- **WHEN** stack 含 service `web`(8080→80) 与 `api`(9090→9090)
- **THEN** 导入后模块「web」含入口 HostPort=8080，模块「api」含入口 HostPort=9090
- **THEN** 修改「web」端口不影响「api」模块

#### Scenario: service 映射为模块
- **WHEN** 导入 stack
- **THEN** 每个 `com.docker.compose.service` 生成一个 AppModule，`SourceService` 保留原 service 名

#### Scenario: 非 HTTP 端口
- **WHEN** 容器发布 MySQL 3306 等非 HTTP 端口
- **THEN** 生成 Kind=Tcp 的入口，预览中默认可勾选，详情页不生成链接

#### Scenario: HostHints
- **WHEN** Portainer endpoint 或配置提供宿主机地址
- **THEN** 预览可将其作为 Host 建议写入全局 Host 目录或应用 HostRefs（用户确认）

### Requirement: 导入不自动覆盖手工内容
再同步 SHALL 默认仅提示漂移（例如 Source 端端口集合变化），不自动删除用户手工修改的模块名或追加入口。

#### Scenario: 检测漂移
- **WHEN** Source 端端口集合与上次导入 SyncHint 不一致
- **THEN** 管理列表或导入页显示「可更新」提示
- **THEN** 需用户确认后才覆盖模块/入口结构
