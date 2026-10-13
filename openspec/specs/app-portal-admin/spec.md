# Purpose

管理后台：应用表格、创建/编辑表单（URL 行 + 快捷生成）、Host 目录、数据源与导入。

## Requirements

### Requirement: 管理后台展示全部应用列表
管理后台 `/Admin` SHALL 以表格展示所有应用，按 `SortOrder` 升序排列；表格内容不被圆角面板裁切，操作列滚动时保持可见。

#### Scenario: 表格展示应用信息
- **WHEN** 用户访问 `/Admin`
- **THEN** 表格列含名称、模块数、来源、排序、状态、操作

#### Scenario: 表格溢出滚动
- **WHEN** 表格内容超出面板宽度
- **THEN** 横向滚动，首列/末列保留内边距
- **THEN** 操作列 sticky 在右侧，不被裁切

### Requirement: 新增应用
管理后台 SHALL 支持新增应用；表单以模块 + URL 字符串行管理地址，至少一条有效 URL。

#### Scenario: 成功新增应用（最少字段）
- **WHEN** 填写名称并至少一个 `http(s)://` URL
- **THEN** 创建应用并跳回管理列表，默认启用

#### Scenario: URL 行操作
- **WHEN** 用户添加/复制/删除地址行
- **THEN** 仅复制 URL 文本，保存后原样存入该模块 Addresses

### Requirement: 编辑应用
管理后台 SHALL 支持编辑已有应用的名称、描述、图标、模块 URL 列表与排序。

#### Scenario: 成功编辑应用
- **WHEN** 修改信息并提交
- **THEN** 更新已有应用，不新建

### Requirement: 删除应用
仅已禁用的应用可删除；已启用应用不可删除。

#### Scenario: 已启用应用不可删除
- **WHEN** 尝试删除已启用应用
- **THEN** 返回管理列表并提示「仅已禁用的应用可以删除」

### Requirement: 管理表单动态地址行
新增和编辑表单 SHALL 以 URL 字符串行为单位管理模块地址，并提供快捷生成弹窗按协议/多 IP/端口/路径批量追加。

#### Scenario: 批量生成多条 URL
- **WHEN** 在生成弹窗中多选 Host 下 IP、协议、端口或路径并确认
- **THEN** 预览全部组合 URL，确认后一次追加到当前模块

#### Scenario: 生成字段约束
- **WHEN** 使用生成器
- **THEN** 协议必选 http 或 https
- **THEN** 地址至少一个
- **THEN** 端口与路径不可同时多选

### Requirement: Host 目录管理
`/Admin/Hosts` SHALL 支持 Host 的创建（Name + 多地址）与删除；地址按行/逗号拆分为多 IP。

#### Scenario: 新增 Host
- **WHEN** 提交 Name 与地址列表
- **THEN** 创建 Host 供生成器与导入改写使用

### Requirement: 数据源与导入入口
`/Admin/Sources` SHALL 列出数据源并支持创建 Portainer 源、测试连接、进入导入、删除。

#### Scenario: 进入导入
- **WHEN** 点击某数据源「导入」
- **THEN** 打开 `/Admin/Import/{sourceId}` 候选列表
