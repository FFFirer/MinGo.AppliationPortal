# Purpose

全局 Host 目录：命名主机可挂多个可达 IP/域名，作为应用地址生成与导入批量改写的候选池。

## Requirements

### Requirement: Host 实体
系统 SHALL 维护全局 Host 目录，每条 Host 含 Name、Addresses（多个 IP/域名）、IsDefault、SortOrder。

#### Scenario: 创建 Host
- **WHEN** 管理员在 `/Admin/Hosts` 通过模态框创建 Host
- **THEN** 必填 Name 与至少一个地址（一行一个，或逗号分隔）
- **THEN** 可标记 IsDefault

#### Scenario: 编辑 Host
- **WHEN** 管理员在 `/Admin/Hosts` 通过模态框编辑已有 Host
- **THEN** 表单回填该 Host 的 Name 与 Addresses
- **THEN** 保存后更新 Name 与 Addresses
- **THEN** 校验规则与创建一致（Name 必填、至少一个地址）

#### Scenario: 多 IP
- **WHEN** Host 配置了多个 Addresses
- **THEN** 导入批量改写时选中该 Host 会使用全部 IP 生成地址

### Requirement: Host 作为生成候选
Create/Edit 快捷生成与导入批量改写 SHALL 将 Host.Addresses 作为 IP 候选，不强制绑定应用级 HostRefs。

#### Scenario: 导入下拉单选 Host
- **WHEN** 导入预览选择主机
- **THEN** 仅能从 Host 目录单选，不提供自定义 IP 输入
- **THEN** 选中后预览展示该 Host 全部 IP

### Requirement: Host 管理模态框
新建与编辑 Host SHALL 共用同一模态框，标题与操作按钮固定在模态框顶部/底部，不随内容滚动。

#### Scenario: 固定标题
- **WHEN** 打开新建模态框
- **THEN** 标题固定显示「新增 Host」
- **WHEN** 打开编辑模态框
- **THEN** 标题固定显示「编辑 Host」

#### Scenario: 固定操作按钮
- **WHEN** 模态框打开
- **THEN** 底部固定显示「取消」与「保存」
- **THEN** 「取消」关闭模态框且不提交
- **THEN** 「保存」提交当前表单

#### Scenario: 关闭模态框
- **WHEN** 管理员点击「取消」或遮罩
- **THEN** 模态框关闭，列表保持原状态
