## ADDED Requirements

### Requirement: 首页展示已启用应用列表
系统首页 SHALL 以卡片网格形式展示所有 `IsEnabled = true` 的应用。应用按 `SortOrder` 升序排列。

#### Scenario: 首页显示已启用应用
- **WHEN** 用户访问门户首页 `/`
- **THEN** 系统展示所有已启用应用的卡片，按 SortOrder 排序

#### Scenario: 禁用应用不展示
- **WHEN** 管理员禁用了某个应用
- **THEN** 该应用不再出现在首页卡片列表中

#### Scenario: 所有应用均被禁用时首页为空
- **WHEN** 所有应用均被禁用
- **THEN** 首页显示空状态提示，如"暂无可用应用"

### Requirement: 应用卡片展示信息
每个应用卡片 SHALL 展示名称、描述（若有）、图标（若有）和所有访问地址链接。

#### Scenario: 卡片显示完整信息
- **WHEN** 应用配置了名称、描述、图标地址和多个访问地址
- **THEN** 卡片显示应用名称、描述文本、图标图片，以及每个地址对应的链接按钮/标签

#### Scenario: 图标地址为空时不显示图标
- **WHEN** 应用未配置图标地址（IconUrl 为空）
- **THEN** 卡片使用默认占位图标或纯文字展示

### Requirement: 访问地址在新标签页打开
每个应用地址链接 SHALL 使用 `target="_blank"` 及 `rel="noopener noreferrer"` 在新标签页中打开。

#### Scenario: 点击链接跳转到目标站点
- **WHEN** 用户点击某个访问地址
- **THEN** 浏览器在新标签页中打开该地址
- **THEN** 门户首页保持打开状态不变

### Requirement: 首页包含管理后台入口
首页 SHALL 提供进入管理后台的入口链接。

#### Scenario: 从首页导航到管理后台
- **WHEN** 用户点击"管理后台"入口
- **THEN** 导航到 `/Admin` 页面
