# Proposal: unify-page-content-width

## Why

各页正文宽度不统一：首页/管理列表走 `page-shell`（1280px），编辑/Hosts/Sources 等走 `page-shell-narrow`（768px），Delete 还有一个裸 `max-w-lg`（512px）。页头页脚固定 1280px，与窄页正文形成「顶栏宽、身体窄」的落差；Hosts、Sources 等表格页被压进表单宽度，列空间不足。需要全站统一内容宽度，让页面节奏一致。

## What Changes

- 正文宽度收敛为**单一档** `page-shell`（`max-w-7xl` = 1280px），与页头/页脚对齐
- 所有页面正文容器改为 `page-shell`：首页、详情、管理列表、新增/编辑/删除、Hosts、Sources、Import
- 废弃 `page-shell-narrow` 的页面级用法（类可保留定义或删除，不再被页面引用）
- 修正 `Admin/Delete.cshtml` 的裸 `max-w-lg mx-auto …` 逃兵写法，改用 `page-shell`
- 表单页（新增/编辑）在 1280px 下保持现有字段结构；模块块与 URL 行随壳层变宽，不另加内部收窄约束
- 不改页头/页脚结构（已使用 `page-shell`），不改卡片/表格组件语义

## Capabilities

### New Capabilities

- `page-content-width`: 全站正文内容宽度约定——单一 `page-shell` 档、与 chrome 对齐、禁止页级裸 max-width 逃逸

### Modified Capabilities

（无；现有 `app-portal-admin` / `app-portal-home` 未把宽度写成行为约束，本次不改其功能需求）

## Impact

- `AppPortal/Styles/app.css` — `page-shell` 保留；`page-shell-narrow` 移除或标记废弃；去掉对其的 `@apply` 拼合引用
- `AppPortal/wwwroot/css/app.css` — 构建产物同步
- `AppPortal/Pages/Index.cshtml` — 已用 `page-shell`，无需改
- `AppPortal/Pages/Detail.cshtml` — `page-shell-narrow` → `page-shell`
- `AppPortal/Pages/Admin/Index.cshtml` — 已用 `page-shell`，无需改
- `AppPortal/Pages/Admin/Create.cshtml` / `Edit.cshtml` — `page-shell-narrow` → `page-shell`
- `AppPortal/Pages/Admin/Delete.cshtml` — 裸 `max-w-lg mx-auto px-4 sm:px-6 py-8` → `page-shell`
- `AppPortal/Pages/Admin/Hosts.cshtml` / `Sources.cshtml` / `Import.cshtml` — `page-shell-narrow` → `page-shell`
- `AppPortal/Pages/Shared/_Layout.cshtml` — 页头/页脚已用 `page-shell`，不改
- 响应式：小屏 padding 规则（`px-4 py-5`）继续挂在 `page-shell` 上，不受影响
