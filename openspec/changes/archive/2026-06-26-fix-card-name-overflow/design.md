## Context

应用卡片标题（h3 标签中的 `@app.Name`）缺少溢出控制，长名称会超出卡片边界。与已修复的 URL 文本行为不一致。

## Goals / Non-Goals

**Goals:**
- 卡片标题文本不超出容器宽度
- 用户悬停可查看完整名称
- 与现有 URL 截断方案行为一致

**Non-Goals:**
- 不修改后端数据模型或逻辑
- 不修改 CSS 定义（复用现有 `truncate`）

## Decisions

| Decision | Choice | Rationale |
|---|---|---|
| 截断方式 | Tailwind `truncate` class | 与 URL 截断完全一致，`app.css` 中已定义 |
| Tooltip | `title` 属性 | 与 URL 方案一致，零依赖 |
| 作用位置 | `<h3>` 标签 | 标题文本的直接容器 |

## Risks / Trade-offs

- 无新风险 — 与已上线的 URL 截断方案完全一致
