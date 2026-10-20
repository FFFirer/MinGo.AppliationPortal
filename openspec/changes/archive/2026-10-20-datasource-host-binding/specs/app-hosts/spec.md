## MODIFIED Requirements

### Requirement: Host 作为生成候选
Create/Edit 快捷生成与导入批量改写 SHALL 将 Host.Addresses 作为 IP 候选，不强制绑定应用级 HostRefs。导入侧候选池受数据源 Host 绑定约束：已绑定则仅绑定 Host，未绑定则全部 Host；主机为多选，默认全选候选池。

#### Scenario: 导入多选 Host
- **WHEN** 导入预览选择主机
- **THEN** 仅能从 Host 目录（或数据源绑定子集）多选，不提供自定义 IP 输入
- **THEN** 默认全选候选池内 Host，预览展示各选中 Host 全部 IP

#### Scenario: 未绑定数据源展示全部 Host
- **WHEN** 当前数据源未绑定 Host
- **THEN** 候选池为全局 Host 目录
