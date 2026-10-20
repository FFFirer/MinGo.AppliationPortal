## ADDED Requirements

### Requirement: 数据源实体
系统 SHALL 支持 `DataSource` 实体：Name、Type(Portainer|ComposeFile|Manual)、Config(JSON)、IsEnabled、LastSyncAt、LastStatus。同一 Type 可有多条数据源（多个 Portainer 实例）。

#### Scenario: 创建 Portainer 数据源
- **WHEN** 管理员在 `/Admin/Sources` 创建 Type=Portainer 的数据源
- **THEN** 必填 Name、BaseUrl、ApiKey（或用户名密码）
- **THEN** 可执行「测试连接」并显示连通结果

#### Scenario: 测试连接失败
- **WHEN** Portainer BaseUrl 或 ApiKey 无效
- **THEN** 显示错误信息，数据源仍可保存但标记为连接失败

### Requirement: 导入管道与 AppDraft
系统 SHALL 通过 `IAppSource` 契约将外部元数据归一为 `AppDraft`，经预览确认后写入 `AppEntry`，并记录 `SourceId` 与 `ExternalId`。

#### Scenario: 列出候选
- **WHEN** 管理员选择某个 Portainer 数据源并进入导入页
- **THEN** 列出该实例下的 stacks（名称、类型、元数据）

#### Scenario: 预览 AppDraft
- **WHEN** 管理员选择某个 stack 进入预览
- **THEN** 展示建议的模块与地址（含端口映射标签），可勾选地址、批量改写后导入
- **THEN** 确认导入后创建或更新应用

#### Scenario: 再导入更新而非重复创建
- **WHEN** 对同一 SourceId+ExternalId 再次导入
- **THEN** 更新已有应用（模块/地址按预览结果写入），不新建第二条

### Requirement: ExternalId 一致性
候选列表、预览 Draft 与表单回传的 `ExternalId` SHALL 使用同一稳定标识（Portainer stack 数字 Id），禁止列表用 Id、Draft 用 Name 导致「未选择有效 stack」。

#### Scenario: 预览后确认导入
- **WHEN** 管理员从候选点「预览」再点「确认导入」
- **THEN** 服务端能按表单中的 SelectedExternalId 命中同一 stack
- **THEN** 不因 Id/名称不一致报「未选择有效 stack」

#### Scenario: 按 Id 或名称兜底解析
- **WHEN** 回传的 ExternalId 为 stack 名称
- **THEN** 仍可通过名称匹配到 stack 并继续导入

### Requirement: Portainer JSON 容错解析
解析 Portainer / Docker API 响应时，系统 SHALL 兼容同一字段的 String 与 Number 形态（例如 stack `Status` 可能为数字或字符串），避免 `GetString`/`GetInt32` 类型异常中断导入。

#### Scenario: Status 为数字
- **WHEN** `GET /api/stacks` 返回 `"Status": 1`
- **THEN** 候选元数据展示为 `status=active`（1=active, 2=inactive, 3=removed）
- **THEN** 不抛出 “requires an element of type 'String' … 'Number'”

#### Scenario: 字段缺失或类型异常
- **WHEN** 字段缺失、null 或类型不匹配
- **THEN** 使用安全默认值继续解析其余 stack，不中断整表

### Requirement: 导入批量改写地址
导入预览 SHALL 支持按「协议 + 主机 + 可选路径」批量生成/改写模块地址；字段顺序为必填在前、可选在后（协议、主机，再路径）。主机候选池按数据源绑定过滤（见「导入 Host 候选池优先绑定」），主机字段为多选且默认全选。

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

### Requirement: 数据源绑定 Host
数据源 SHALL 支持绑定零条或多条 Host（`HostIds`），在创建与编辑入口均可维护。绑定是导入 Host 候选池的硬约束：有绑定则只使用绑定集合，无绑定则回落全局 Host 目录。

#### Scenario: 创建时绑定 Host
- **WHEN** 管理员创建数据源并勾选若干 Host
- **THEN** 数据源保存 `HostIds` 为所选 Host 的 Id 列表
- **WHEN** 未勾选任何 Host
- **THEN** `HostIds` 为空，表示未绑定

#### Scenario: 编辑绑定
- **WHEN** 管理员编辑已有数据源的 Host 绑定
- **THEN** 可增删勾选并保存
- **THEN** 下次进入该数据源的导入预览使用新的候选池

#### Scenario: 绑定 Host 已被删除
- **WHEN** 数据源 `HostIds` 中含已不存在的 Host Id
- **THEN** 导入候选池过滤掉悬空 Id
- **WHEN** 过滤后绑定集合为空
- **THEN** 视同未绑定，展示全部 Host 并提示绑定已失效

### Requirement: 导入 Host 候选池优先绑定
导入预览的主机候选 SHALL 优先取数据源绑定的 Host；数据源未绑定任何有效 Host 时 SHALL 展示全局 Host 目录。候选一律来自 Host 目录，不提供自定义 IP 输入。

#### Scenario: 已绑定则硬过滤
- **WHEN** 数据源绑定了 Host A、B
- **THEN** 导入「批量改写地址」仅列出 A、B，不列出未绑定的 C

#### Scenario: 未绑定则展示全部
- **WHEN** 数据源 `HostIds` 为空
- **THEN** 列出全部 Host（与现行为一致）

#### Scenario: 多选 Host
- **WHEN** 导入预览选择主机
- **THEN** 可同时勾选多个候选 Host
- **THEN** 默认全选候选池内全部 Host

### Requirement: Portainer stack 端口独立识别
从 Portainer 导入时，系统 SHALL 将每个 compose service 的端口仅落入该 service 对应模块，不得混入其他模块。

#### Scenario: 多 service 独立端口
- **WHEN** stack 含 service `web`(8080→80) 与 `api`(9090→9090)
- **THEN** 导入后模块「web」仅含 web 端口地址，模块「api」仅含 api 端口地址

#### Scenario: service 映射为模块
- **WHEN** 导入 stack
- **THEN** 每个 `com.docker.compose.service` 生成一个 AppModule，`SourceService` 保留原 service 名

### Requirement: 导入不自动覆盖手工内容
再同步 SHALL 默认仅提示漂移（例如 Source 端端口集合变化），不自动删除用户手工修改的模块名或追加地址。

#### Scenario: 检测漂移
- **WHEN** Source 端端口集合与上次导入 SyncHint 不一致
- **THEN** 管理列表或导入页显示「可更新」提示
- **THEN** 需用户确认后才覆盖模块/地址结构
