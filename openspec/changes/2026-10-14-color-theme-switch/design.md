# Design: color-theme-switch

## Context

技术栈：ASP.NET Core Razor Pages + Tailwind CSS v4（CLI：`Styles/app.css` → `wwwroot/css/app.css`）。

现状：

- 自定义 `@layer components`（`.card` / `.btn-*` / `.data-table` / `.input` / `.badge-*` 等）全部浅色
- 页面工具类约 153 处硬编码颜色；CSS 源约 89 处颜色引用
- 编译产物与源 CSS 均无 `dark:` 变体
- `body` 背景为三条浅色 radial-gradient + `#f6f7fb`
- 粘性表列 / 滚动条使用 raw `rgb()`，无法靠工具类翻转
- `Create` / `Edit` / `Import` 的 `innerHTML` 模板硬编码浅色类名
- 无主题脚本、无偏好存储

目标：纯客户端主题切换 + 首帧防闪烁，不改业务与数据层。

## Goals / Non-Goals

**Goals:**

- 浅色 / 暗色 / 跟随系统 三态切换，偏好 `localStorage` 持久化
- 首帧即正确主题（内联脚本先于 CSS 应用 `html.dark`）
- 组件层、页面、JS 动态模板视觉一致可读
- 原生控件 / 滚动条 / `::selection` 跟随 `color-scheme` 与主题色

**Non-Goals:**

- 不做服务端主题 Cookie / 用户账号级偏好
- 不做多套配色（仅 light / dark 两套）
- 不重构组件库、不引入 CSS-in-JS / 主题框架
- 不改业务路由、模型、API

## Decisions

1. **Tailwind v4 class 驱动（`html.dark`），不用 `prefers-color-scheme` 媒体查询作为唯一源**  
   在 `Styles/app.css` 增加 `@custom-variant dark (&:where(.dark, .dark *));`。  
   备选：纯 media query —— 无法被按钮覆盖，且无法记住用户选择，放弃。  
   说明：未存偏好时由内联脚本读取 `prefers-color-scheme` 决定初始 class，仍可「跟随系统」。

2. **语义 CSS 变量承载色板，组件层优先引用变量**  
   在 `:root` 定义 `--color-bg`、`--color-surface`、`--color-surface-2`、`--color-border`、`--color-ink`、`--color-ink-muted`、`--color-ink-faint` 等；`.dark` 覆盖为暗色值。  
   raw rgb（粘性列、滚动条、分隔线）改为 `var(...)`。  
   备选：为每条规则手写 `dark:bg-gray-900` 等 —— 可行但 raw rgb 覆盖不到、后续难调，变量更稳。

3. **页面工具类补 `dark:` 变体（成对书写）**  
   例如 `bg-white` → `bg-white dark:bg-gray-900`，`text-gray-900` → `text-gray-900 dark:text-gray-100`。  
   语义映射（内部约定）：
   - 表面：`white` / `gray-50` → `gray-900` / `gray-800`
   - 描边：`gray-200` / `gray-100` → `gray-700` / `gray-800`
   - 正文：`gray-900` / `slate-800` → `gray-100`
   - 弱文：`gray-500` / `gray-400` → `gray-400` / `gray-500`
   - 强调蓝：保留 `blue-*`，hover 略提亮（`blue-400` 等）
   - 警示/成功/信息 badge 与 alert：背景改深色半透明，文字提亮

4. **JS 动态模板与静态页同步改类名**  
   `Create` / `Edit` 的模块块、地址行、预览 chip，以及 `Import` 的 IP chip 字符串，都写入同样的 `dark:` 对。  
   备选：JS 生成后只用组件类 —— 大改模板收益有限，本次对齐工具类即可。

5. **防闪烁：`<head>` 内联同步脚本 + `localStorage` 三态**  
   存储键 `theme`：`light` | `dark` | `system`（或缺省 = system）。  
   脚本在 stylesheet 之前执行：解析偏好 → 需要时 `documentElement.classList.add('dark')`。  
   顶栏按钮循环切换三态（或用简单三档按钮），写入 storage 并即时切换 class。  
   `html` 设置 `color-scheme`（light 时 `light`，dark 时 `dark`，system 时 `light dark`）。

6. **补齐缺失的 `btn-ghost` / `field-hint`**  
   页面已引用但 CSS 未定义。`btn-ghost` 为无边框次要按钮；`field-hint` 为表单辅助小字。一并纳入组件层并带 dark 变体，避免主题下出现裸样式。

## Risks / Trade-offs

- [工具类成对遗漏导致局部刺眼] → 优先改组件层（全局生效），页面按文件清单过一遍；Create/Edit 动态模板单独点名
- [Tailwind 扫不到 JS 字符串里的类] → 动态模板里的类名同时出现在 cshtml 静态部分或 `@source` 覆盖范围内；必要时在 CSS 用 `@source inline(...)` 保底（优先手写进已有静态类）
- [首帧闪烁] → 内联脚本必须在 `<link rel="stylesheet">` 之前；禁止把初始化放到外部 `app.js` 或 `DOMContentLoaded`
- [滚动条 / 粘性列 raw 色] → 全部改为语义变量，避免二次遗漏
- [三态按钮可发现性] → 顶栏图标按钮 + `title`/`aria-label` 标明当前态；不引入设置页

## Migration Plan

1. 改 `Styles/app.css` 与 `_Layout.cshtml`，再改各页与 JS 模板，一次 `css:build` 发布
2. 回滚 = 还原上述文件并重建 CSS；无数据迁移

## Open Questions

无。
