# color-theme

## ADDED Requirements

### Requirement: 主题模式切换
门户 SHALL 支持浅色、暗色、跟随系统三种主题模式，用户可通过顶栏控件切换。

#### Scenario: 切换到暗色
- **WHEN** 用户选择暗色主题
- **THEN** 页面立即以暗色渲染，无需刷新
- **THEN** 偏好写入 `localStorage`

#### Scenario: 切换到浅色
- **WHEN** 用户选择浅色主题
- **THEN** 页面立即以浅色渲染，无需刷新

#### Scenario: 跟随系统
- **WHEN** 用户选择「跟随系统」或从未设置过偏好
- **THEN** 主题与 `prefers-color-scheme` 一致
- **WHEN** 系统主题在「跟随系统」下变化
- **THEN** 页面主题随之更新

### Requirement: 偏好持久化
主题选择 SHALL 存储在浏览器 `localStorage`，跨页面、跨会话保持。

#### Scenario: 刷新后保持
- **WHEN** 用户选择暗色后刷新或打开其他页面
- **THEN** 仍为暗色，且首帧无浅色闪烁

### Requirement: 首帧防闪烁
在样式表应用前，SHALL 根据已存偏好或系统偏好设置文档根主题类，避免主题闪变（FOUC）。

#### Scenario: 硬刷新无闪烁
- **WHEN** 用户在暗色偏好下硬刷新任意页面
- **THEN** 首帧即为暗色，不出现浅色底闪现

### Requirement: 界面可读性
暗色下卡片、按钮、输入、表格、徽章、弹窗、表单控件与滚动条 SHALL 保持足够对比度与可辨识度。浏览器主题色（`theme-color`）SHALL 随当前主题模式同步。

#### Scenario: 组件暗色可读
- **WHEN** 以暗色浏览首页、详情、管理页与弹窗
- **THEN** 正文、弱文、描边、表面层次清晰可辨
- **THEN** 原生控件与滚动条随 `color-scheme` 呈暗色

#### Scenario: theme-color 随主题
- **WHEN** 主题为浅色
- **THEN** `theme-color` 为 `#2563eb`
- **WHEN** 主题为暗色
- **THEN** `theme-color` 为 `#111827`
- **WHEN** 用户切换主题
- **THEN** `theme-color` 立即更新
