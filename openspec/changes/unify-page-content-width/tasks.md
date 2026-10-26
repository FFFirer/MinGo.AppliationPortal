# Tasks: unify-page-content-width

## 1. 样式层：单一壳层

- [x] 1.1 `AppPortal/Styles/app.css` 删除 `.page-shell-narrow` 定义，保留 `.page-shell`
- [x] 1.2 同步清理小屏媒体查询里对 `.page-shell-narrow` 的拼合选择器，保证 `page-shell` 响应式间距完整

## 2. 页面层：全部并入 page-shell

- [x] 2.1 `Detail.cshtml`、`Admin/Create.cshtml`、`Admin/Edit.cshtml`：`page-shell-narrow` → `page-shell`
- [x] 2.2 `Admin/Hosts.cshtml`、`Admin/Sources.cshtml`、`Admin/Import.cshtml`：`page-shell-narrow` → `page-shell`
- [x] 2.3 `Admin/Delete.cshtml`：裸 `max-w-lg mx-auto px-4 sm:px-6 py-8` → `page-shell`；确认卡内部保留居中窄卡（`max-w-lg mx-auto`）

## 3. 构建与验证

- [x] 3.1 重建 `wwwroot/css/app.css`，确认产物含 `page-shell`、不含 `page-shell-narrow`
- [x] 3.2 全库检索 `page-shell-narrow` 与页级裸 `max-w-lg mx-auto px-4` 无残留
- [ ] 3.3 走查首页 / 管理列表 / 详情 / 新增 / 编辑 / 删除 / Hosts / Sources / Import：壳层同宽、无横向滚动、小屏间距正常（需重启正在运行的 AppPortal 后人工确认）
