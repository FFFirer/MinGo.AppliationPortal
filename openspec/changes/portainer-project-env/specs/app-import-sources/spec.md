# Delta: app-import-sources

## MODIFIED Requirements

### Requirement: 数据源实体
系统 SHALL 支持 `DataSource` 实体：Name、Type(Portainer|ComposeFile|Manual)、Config(JSON)、IsEnabled。同一 Type 可有多条数据源（多个 Portainer 实例）。Host 绑定与同步状态 SHALL 由 `PortainerEnvironment` 承载。

#### Scenario: 创建 Portainer 数据源
- **WHEN** 管理员在 `/Admin/Sources` 创建 Type=Portainer 的数据源
- **THEN** 必填 Name、BaseUrl、ApiKey（或用户名密码）
- **THEN** 可执行「测试连接」并显示连通结果

#### Scenario: 测试连接失败
- **WHEN** Portainer BaseUrl 或 ApiKey 无效
- **THEN** 显示错误信息，数据源仍可保存

### Requirement: 导入管道与 AppDraft
系统 SHALL 通过 `IAppSource` 契约将外部元数据归一为 `AppDraft`，经预览确认后写入 `AppEntry`，并记录 `SourceId`、`EnvironmentId` 与 `ExternalId`。

#### Scenario: 列出候选
- **WHEN** 管理员从某个 Environment 进入导入页
- **THEN** 列出该 Environment 下的 compose projects（Portainer 管理的 stack 与原生 `docker compose up` 项目均包含）

#### Scenario: 预览 AppDraft
- **WHEN** 管理员选择某个 compose project 进入预览
- **THEN** 展示建议的模块与地址（含端口映射标签），可勾选地址、批量改写后导入
- **THEN** 确认导入后创建或更新应用

#### Scenario: 再导入更新而非重复创建
- **WHEN** 对同一 `SourceId + EnvironmentId + ExternalId` 再次导入
- **THEN** 更新已有应用（模块/地址按预览结果写入），不新建第二条

### Requirement: ExternalId 一致性
候选列表、预览 Draft 与表单回传的 `ExternalId` SHALL 使用同一稳定标识（compose project name），禁止列表与 Draft 使用不同标识导致「未选择有效 compose project」。

#### Scenario: 预览后确认导入
- **WHEN** 管理员从候选点「预览」再点「确认导入」
- **THEN** 服务端能按表单中的 SelectedExternalId 命中同一 project
- **THEN** 不因标识不一致报错

#### Scenario: 按名称兜底解析
- **WHEN** 回传的 ExternalId 与候选名称大小写不同
- **THEN** 仍可大小写不敏感地匹配并继续导入

### Requirement: 导入批量改写地址
导入预览 SHALL 支持按「协议 + 主机 + 可选路径」批量生成/改写模块地址；字段顺序为必填在前、可选在后（协议、主机，再路径）。主机候选池按 Environment 绑定过滤，主机字段为多选且默认全选。

#### Scenario: 字段顺序
- **WHEN** 打开导入预览的「批量改写地址」
- **THEN** 展示顺序为：协议\*、主机\*（多选，含已选 IP 预览）、路径（可选）

#### Scenario: 多选 Host 并展开全部 IP
- **WHEN** 勾选一个或多个 Host（不可自定义 IP）
- **THEN** 预览区展示各选中 Host 的全部 IP（跨 Host 去重）
- **THEN** 点「按此配置刷新地址」后，每个服务端口展开为「每个去重 IP 一条 URL」，默认全选

#### Scenario: 保留手工勾选
- **WHEN** 确认导入前用户取消勾选部分地址或改过 URL
- **THEN** 仅导入仍勾选的地址，URL 以表单值为准

### Requirement: 导入 Host 候选池优先绑定
导入预览的主机候选 SHALL 优先取当前 Environment 绑定的 Host；Environment 未绑定任何有效 Host 时 SHALL 展示全局 Host 目录。候选一律来自 Host 目录，不提供自定义 IP 输入。

#### Scenario: 已绑定则硬过滤
- **WHEN** Environment 绑定了 Host A、B
- **THEN** 导入「批量改写地址」仅列出 A、B，不列出未绑定的 C

#### Scenario: 未绑定则展示全部
- **WHEN** Environment `HostIds` 为空
- **THEN** 列出全部 Host

#### Scenario: 多选 Host
- **WHEN** 导入预览选择主机
- **THEN** 可同时勾选多个候选 Host
- **THEN** 默认全选候选池内全部 Host

### Requirement: compose project 端口独立识别
从 Portainer 导入时，系统 SHALL 将每个 compose service 的端口仅落入该 service 对应模块，不得混入其他模块。

#### Scenario: 多 service 独立端口
- **WHEN** project 含 service `web`(8080→80) 与 `api`(9090→9090)
- **THEN** 导入后模块「web」仅含 web 端口地址，模块「api」仅含 api 端口地址

#### Scenario: service 映射为模块
- **WHEN** 导入 project
- **THEN** 每个 `com.docker.compose.service` 生成一个 AppModule，`SourceService` 保留原 service 名

### Requirement: 导入不自动覆盖手工内容
再同步 SHALL 默认仅提示漂移（例如 Source 端端口集合变化），不自动删除用户手工修改的模块名或追加地址。

#### Scenario: 检测漂移
- **WHEN** Source 端端口集合与上次导入 SyncHint 不一致
- **THEN** 管理列表或导入页显示「可更新」提示
- **THEN** 需用户确认后才覆盖模块/地址结构
