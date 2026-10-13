# Purpose

全局 Host 目录：命名主机可挂多个可达 IP/域名，作为应用地址生成与导入批量改写的候选池。

## Requirements

### Requirement: Host 实体
系统 SHALL 维护全局 Host 目录，每条 Host 含 Name、Addresses（多个 IP/域名）、IsDefault、SortOrder。

#### Scenario: 创建 Host
- **WHEN** 管理员在 `/Admin/Hosts` 创建 Host
- **THEN** 必填 Name 与至少一个地址（一行一个，或逗号分隔）
- **THEN** 可标记 IsDefault

#### Scenario: 多 IP
- **WHEN** Host 配置了多个 Addresses
- **THEN** 导入批量改写时选中该 Host 会使用全部 IP 生成地址

### Requirement: Host 作为生成候选
Create/Edit 快捷生成与导入批量改写 SHALL 将 Host.Addresses 作为 IP 候选，不强制绑定应用级 HostRefs。

#### Scenario: 导入下拉单选 Host
- **WHEN** 导入预览选择主机
- **THEN** 仅能从 Host 目录单选，不提供自定义 IP 输入
- **THEN** 选中后预览展示该 Host 全部 IP
