## MODIFIED Requirements

### Requirement: 应用卡片展示信息
每个应用卡片 SHALL 展示名称、描述（若有）、图标（若有）。卡片不展示访问地址链接；点击卡片进入应用详情页。

#### Scenario: 卡片显示身份信息
- **WHEN** 配置了名称、描述、图标地址
- **THEN** 卡片显示应用名称、描述文本、图标图片

#### Scenario: 图标地址为空时不显示图标
- **WHEN** 应用未配置图标地址（IconUrl 为空）
- **THEN** 卡片使用默认占位图标或纯文字展示

#### Scenario: 卡片不展示链接
- **WHEN** 应用已配置模块与入口
- **THEN** 首页卡片不渲染任何跳转 URL 或链接列表

#### Scenario: 点击卡片进入详情
- **WHEN** 用户点击应用卡片
- **THEN** 导航到该应用详情页 `/Detail/{id}`

### Requirement: 访问地址在新标签页打开
应用访问地址 SHALL 在详情页的入口链接中使用 `target="_blank"` 及 `rel="noopener noreferrer"` 打开（Http 入口）。首页卡片不再直接提供外链。

#### Scenario: 详情页点击链接跳转
- **WHEN** 用户在详情页点击某个 Http 入口
- **THEN** 浏览器在新标签页中打开该地址
- **THEN** 详情页保持打开状态不变
