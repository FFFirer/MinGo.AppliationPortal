## Context

应用卡片（Index.cshtml）中的 URL 链接在 DisplayName 未设置时直接显示完整原始 URL，超出卡片宽度时溢出边界。

## Goals / Non-Goals

**Goals:**
- URL 文本不超出卡片容器边界
- 用户仍能通过 tooltip 看到完整 URL

**Non-Goals:**
- 不修改后端数据模型或逻辑
- 不修改现有 CSS 定义

## Decisions

| Decision | Choice | Rationale |
|---|---|---|
| 截断方式 | Tailwind `truncate` class | 已在 `app.css` 中定义 (`overflow: hidden; text-overflow: ellipsis; white-space: nowrap;`)，无需新增样式 |
| 完整 URL 查看 | HTML `title` 属性 | 浏览器原生 tooltip，零依赖，悬停即显示 |
| 作用范围 | 仅 `<span>` 和 `<a>` 标签 | 最小改动，不影响卡片其他部分 |

## Risks / Trade-offs

- 长 URL 被截断后用户需要悬停才能看到完整地址 — 通过 `title` 属性补偿，浏览器原生 tooltip 体验良好
- 单行截断可能隐藏路径细节 — 但原始问题正是显示完整路径破坏布局，截断是必要让步
