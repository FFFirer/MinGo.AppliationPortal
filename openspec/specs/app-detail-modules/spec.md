# Purpose

应用详情页：按模块分区展示 URL 字符串入口，支持新标签打开。

## Requirements

### Requirement: 应用详情页展示模块入口
系统 SHALL 提供 `/Detail/{id}`，按 `AppModule` 分区展示模块下的 URL 字符串入口。

#### Scenario: 打开应用详情
- **WHEN** 用户在首页点击某个应用卡片
- **THEN** 导航到该应用详情页，展示名称、描述、图标（若有）

#### Scenario: 按模块分组展示
- **WHEN** 应用包含多个模块
- **THEN** 按模块 Order 升序分节，节内列出该模块 Addresses 中的 URL

#### Scenario: 禁用模块不展示
- **WHEN** 某模块 `IsEnabled = false`
- **THEN** 详情页不展示该模块及其地址

#### Scenario: 链接新标签打开
- **WHEN** 用户点击某个 `http(s)://` 地址
- **THEN** 浏览器新标签打开（`target="_blank"` + `rel="noopener noreferrer"`）
