## Context

ASP.NET Core model binder 尝试将路由字符串 `"64a1b2c3..."` 绑定到 `ObjectId` 参数时，因缺少标准绑定机制而静默失败，`ObjectId id` 得到默认随机值，导致所有通过 ID 查找的页面 handler 返回 404。

## Decisions

统一将所有 handler 的 `ObjectId id` 参数改为 `string id`，在方法体内用 `ObjectId.TryParse` 解析：

```csharp
// Before
public async Task<IActionResult> OnGetAsync(ObjectId id)

// After  
public async Task<IActionResult> OnGetAsync(string id)
{
    if (!ObjectId.TryParse(id, out var oid))
        return NotFound();
    var app = await _appService.GetByIdAsync(oid);
    ...
}
```

## Risks / Trade-offs

无风险。纯参数类型变更，手动解析保证正确性。
