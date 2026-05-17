## Why

所有 Admin 页面 handler 使用 `LiteDB.ObjectId` 作为路由参数类型，但 ASP.NET Core 无法自动将 URL 字符串绑定为 `ObjectId`（缺少 `TryParse`/`IParsable`/`TypeConverter`）。导致点击编辑链接后 `id` 为默认值，`FindById` 返回 null，最终 404。

## What Changes

1. **Edit.cshtml.cs** — `OnGetAsync` 和 `OnPostAsync` 参数从 `ObjectId id` 改为 `string id`，手动解析
2. **Delete.cshtml.cs** — `OnGetAsync` 和 `OnPostAsync` 同样改为 `string id` + 手动解析
3. **Index.cshtml.cs** — `OnPostToggleAsync` 同样改为 `string id` + 手动解析

## Capabilities

### New Capabilities
- _无新增_

### Modified Capabilities
- _无 spec 级变更（纯 bugfix，不改需求）_

## Impact

- 3 个 handler 文件，5 个方法签名变更
- 功能行为不变，仅修复参数绑定
