## Why

应用卡片标题（h3 中的 `@app.Name`）过长时同样会溢出卡片边界。上次修复已处理 URL 文本截断，但卡片名称仍未添加溢出控制，显示效果不一致。

## What Changes

- 在 Index.cshtml 卡片标题 `<h3>` 上添加 `truncate` 类，超长名称截断显示省略号
- 在 `<h3>` 上添加 `title` 属性，悬停 tooltip 显示完整名称

## Capabilities

### New Capabilities
- `card-name-display`: 应用卡片标题的显示控制能力，包括文本截断和 tooltip 提示

### Modified Capabilities

<!-- 无现有 spec 需要修改 -->

## Impact

- 仅修改前端视图文件 `AppPortal/Pages/Index.cshtml`，不涉及后端逻辑
- CSS class `truncate` 已在 `wwwroot/css/app.css` 中定义，无需新增样式
- 与 URL 截断方案保持一致，卡片内文本溢出行为统一
