## MODIFIED Requirements

### Requirement: 管理表单动态地址行
新增和编辑表单 SHALL 以 URL 字符串行为单位管理模块地址：每行一个完整 URL，支持添加、复制、删除；并提供快捷生成弹窗按配置批量追加多条 URL。

#### Scenario: 直接输入 URL
- **WHEN** 用户在地址行输入完整 URL
- **THEN** 保存后原样存入该模块 Addresses

#### Scenario: 复制地址行
- **WHEN** 用户点击某行「复制」
- **THEN** 立即插入内容相同的新行，仅复制 URL 文本
- **THEN** 不关联 Host 等结构化字段

#### Scenario: 批量生成多条 URL
- **WHEN** 在生成弹窗中选择多个地址（如 Host 下多 IP）、一个协议、一个或多个端口或路径，并确认
- **THEN** 预览将生成的全部 URL
- **THEN** 确认后一次性追加到当前模块地址列表

#### Scenario: 生成字段约束
- **WHEN** 使用生成器
- **THEN** 协议必选 http 或 https
- **THEN** 地址至少选择或填写一个
- **THEN** 端口与路径不可同时多选（避免组合爆炸）
