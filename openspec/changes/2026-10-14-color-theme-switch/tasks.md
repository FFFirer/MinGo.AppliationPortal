# Tasks: color-theme-switch

## 1. 样式基座

- [x] 1.1 `Styles/app.css`：`@custom-variant dark`，`:root` / `.dark` 语义色变量，`color-scheme`
- [x] 1.2 `body` 背景、`::selection`、滚动条、粘性列 raw rgb 改为变量或 dark 覆盖
- [x] 1.3 组件层（`.card` / `.btn-*` / `.input` / `.data-table` / `.badge-*` / `.alert-*` / `.empty-*` / `.host-chip-*` / `.module-*` 等）补 dark 变体
- [x] 1.4 补齐 `btn-ghost`、`field-hint`（含 dark）

## 2. 页面与动态模板

- [x] 2.1 `_Layout.cshtml`：header / footer / 导航工具类补 dark
- [x] 2.2 首页、详情、Admin 各页工具类补 dark（Index / Detail / Admin/*）
- [x] 2.3 `Create` / `Edit` / `Import` 内联 JS 模板类名同步 dark 对

## 3. 主题切换与防闪烁

- [x] 3.1 `_Layout.cshtml` `<head>` 内联防闪烁脚本（localStorage / prefers-color-scheme）
- [x] 3.2 顶栏主题切换按钮（light / dark / system），写 storage 并切换 `html.dark`
- [x] 3.3 切换后 `color-scheme` 与按钮图标/无障碍标签更新

## 4. 构建与验证

- [x] 4.1 `npm run css:build` 产出 `wwwroot/css/app.css`，确认含 dark 变体（31 个 unique dark: 工具类）
- [x] 4.2 目测浅色/暗色两套下：首页、Hosts 表格+模态、Admin 表格、Create 表单
- [x] 4.3 硬刷新无闪烁；切换后刷新保持；三态循环 light → system → dark 正常
