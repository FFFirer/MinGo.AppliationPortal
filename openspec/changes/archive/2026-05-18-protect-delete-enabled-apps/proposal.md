## Why

当前管理后台的删除操作没有任何防护，任何状态的应用（包括已启用的）都可以被删除。需要增加保护逻辑：只有已禁用的应用才允许删除，防止误操作导致正在使用的应用被意外删除。

## What Changes

1. **Delete.cshtml.cs** — `OnGet` 和 `OnPost` 增加 `IsEnabled` 检查，已启用的应用跳转回列表并提示
2. **Index.cshtml** — 删除链接仅在应用禁用状态时显示
3. **Delete.cshtml** — 添加"仅已禁用的应用可以删除"提示文字
4. **app-portal-admin/spec.md** — 更新删除应用需求，增加禁用保护场景

## Capabilities

### New Capabilities
- _无新增 capability_

### Modified Capabilities
- `app-portal-admin`: 删除应用需求增加禁用保护约束

## Impact

- **Delete.cshtml.cs**: OnGet 中检查 IsEnabled，已启用则重定向；OnPost 同样检查防止绕过
- **Index.cshtml**: 删除按钮增加 `@if (!app.IsEnabled)` 条件渲染
- **Delete.cshtml**: 页面添加禁用提示说明
- **app-portal-admin/spec.md**: 更新需求描述和新增场景
