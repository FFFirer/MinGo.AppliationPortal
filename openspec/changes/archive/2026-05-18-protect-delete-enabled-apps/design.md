## Context

当前管理后台的删除操作无状态保护。`Delete.cshtml.cs` 的 `OnGet` 和 `OnPost` 均直接执行删除或显示确认页，未检查 `AppEntry.IsEnabled` 字段。需要增加两层防护：UI 层（隐藏删除入口）和后端层（重定向/阻止删除）。

## Goals / Non-Goals

**Goals:**
- 后端 `OnGet` 检查 `IsEnabled`，已启用则跳转回列表
- 后端 `OnPost` 检查 `IsEnabled`，防止直接 POST 绕过
- 前端 `Index.cshtml` 隐藏已启用应用的删除按钮
- `Delete.cshtml` 添加禁用提示

**Non-Goals:**
- 不改动 `AppService.DeleteAsync`（保持服务层简单，由页面层控制逻辑）
- 不改动其他 CRUD 操作（Create/Edit/Toggle）
- 不修改数据模型

## Decisions

### Decision 1: 后端双重检查

```
请求删除 ──→ OnGet 检查 IsEnabled
                ├── 已启用 → RedirectToPage("Index", error)
                └── 已禁用 → 显示确认页

确认删除 ──→ OnPost 检查 IsEnabled
                ├── 已启用 → RedirectToPage("Index", error)
                └── 已禁用 → DeleteAsync → RedirectToPage("Index")
```

即使 OnGet 做了检查，OnPost 也要再做一次——防止用户直接从 `/Admin/Delete/{id}` POST 到服务端。

### Decision 2: UI 层隐藏入口

Index 列表的删除链接：
```
@if (!app.IsEnabled)
{
    <a asp-page="Delete" asp-route-id="@app.Id">删除</a>
}
```

已禁用的应用显示灰色删除按钮，已启用的不显示。

## Risks / Trade-offs

| 风险 | 缓解 |
|------|------|
| 用户看不到删除入口不知道必须先禁用 | Delete.cshtml 页面顶部添加提示文字 |
| 误删已禁用的应用 | 保留确认页流程，两步确认后才执行删除 |
