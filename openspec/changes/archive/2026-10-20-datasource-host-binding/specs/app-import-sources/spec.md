## ADDED Requirements

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

## MODIFIED Requirements

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
