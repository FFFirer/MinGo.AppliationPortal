# Delta: color-theme

## MODIFIED Requirements

### Requirement: 界面可读性
暗色下卡片、按钮、输入、表格、徽章、弹窗、表单控件与滚动条 SHALL 保持足够对比度与可辨识度。浏览器主题色（`theme-color`）SHALL 随当前主题模式同步。

#### Scenario: 组件暗色可读
- **WHEN** 暗色下查看卡片、表格、表单
- **THEN** 文字与背景对比度足够

#### Scenario: theme-color 随主题
- **WHEN** 主题为浅色
- **THEN** `theme-color` 为 `#2563eb`
- **WHEN** 主题为暗色
- **THEN** `theme-color` 为 `#111827`
- **WHEN** 用户切换主题
- **THEN** `theme-color` 立即更新
