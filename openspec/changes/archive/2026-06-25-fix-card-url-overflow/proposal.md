## Why

应用卡片中的访问 URL 过长时（常见于 DisplayName 未设置，直接显示原始 URL），文本会溢出卡片容器边界，破坏卡片圆角效果和整体布局美观。

## What Changes

- 在 Index.cshtml 的 URL 链接 `<span>` 上添加 `truncate` CSS 类，使超长 URL 文本截断并显示 `...`
- 在 `<a>` 标签上添加 `title` 属性，值为完整 URL，悬停时以 tooltip 显示完整地址

## Capabilities

### New Capabilities
- `card-url-display`: 应用卡片中 URL 的显示控制能力，包括文本截断和 tooltip 提示

### Modified Capabilities

<!-- 无现有 spec 需要修改 -->

## Impact

- 仅修改前端视图文件 `AppPortal/Pages/Index.cshtml`，不涉及后端逻辑
- CSS class `truncate` 已在 `wwwroot/css/app.css` 中定义，无需新增样式
