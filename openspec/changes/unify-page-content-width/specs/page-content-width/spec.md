## ADDED Requirements

### Requirement: 全站正文统一内容宽度

应用门户所有页面的正文内容区 SHALL 使用同一内容宽度档 `page-shell`（`max-w-7xl`，与页头/页脚对齐），不得因页面类型使用更窄或更宽的页级容器。

#### Scenario: 列表页与表单页壳层一致

- **WHEN** 用户在同视口下先后访问管理列表（`/Admin`）与编辑页（`/Admin/Edit/{id}`）
- **THEN** 两页正文内容区最大宽度相同，且与页头/页脚内容区对齐

#### Scenario: 表格页获得完整壳层宽度

- **WHEN** 用户访问 Host 目录（`/Admin/Hosts`）或数据源（`/Admin/Sources`）
- **THEN** 表格面板与应用管理列表处于同一宽度档，不再收窄至表单宽度

### Requirement: 禁止页级宽度逃逸

页面正文最外层容器 SHALL 使用 `page-shell` 类，MUST NOT 以裸 `max-w-*` + 自定义 padding 替代页级壳层。内容卡片如需更窄，SHALL 在 `page-shell` 内部约束自身宽度。

#### Scenario: 删除页并入统一壳层

- **WHEN** 用户访问删除确认页（`/Admin/Delete/{id}`）
- **THEN** 页级容器为 `page-shell`，不再使用裸 `max-w-lg mx-auto` 作为页面壳层
- **THEN** 确认卡片自身可在壳层内居中收窄

### Requirement: 废弃窄壳层

`page-shell-narrow` SHALL 从样式与页面引用中移除；样式源中不得保留未使用的窄壳层定义。

#### Scenario: 无窄壳层残留

- **WHEN** 在 `AppPortal/Styles` 与 `AppPortal/Pages` 中检索 `page-shell-narrow`
- **THEN** 无匹配

#### Scenario: 小屏间距仍由统一壳层提供

- **WHEN** 视口宽度小于断点
- **THEN** `page-shell` 仍提供紧凑的水平/垂直内边距，不因移除窄壳层而丢失响应式间距
