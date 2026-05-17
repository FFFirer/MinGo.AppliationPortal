## 1. 后端 Delete.cshtml.cs 增加防护

- [x] 1.1 `OnGet` 中检查 `IsEnabled`，已启用则重定向到 Index 并传递提示信息
- [x] 1.2 `OnPost` 中检查 `IsEnabled`，防止直接 POST 绕过

## 2. 前端 UI 调整

- [x] 2.1 `Index.cshtml` 中删除链接增加 `@if (!app.IsEnabled)` 条件渲染
- [x] 2.2 `Delete.cshtml` 页面顶部添加警示提示
- [x] 2.3 `Index.cshtml` 顶部添加 TempData Error 提示条

## 3. 同步 OpenSpec Specs

- [x] 3.1 将 delta spec 同步到主 spec `app-portal-admin/spec.md`
