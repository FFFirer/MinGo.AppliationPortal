# Proposal: color-theme-switch

## Why

门户目前仅有浅色外观：`Styles/app.css` 组件层与各页工具类全部写死浅色（`bg-white` / `text-gray-*` / 浅色渐变背景），无 `dark:` 变体、无主题偏好存储。夜间或暗光环境刺眼，且原生控件与滚动条也不会跟随。需要可切换的浅色/暗色主题，纯客户端实现，首帧无闪烁。

## What Changes

- 启用 Tailwind CSS v4 class 驱动暗色变体（`html.dark`）
- 抽出语义色 CSS 变量（背景、表面、描边、正文、弱文），组件层与 raw rgb 改为引用变量
- 为 `@layer components` 组件与页面工具类补齐 `dark:` 变体（含 JS 动态生成模板）
- `<head>` 内联脚本在首帧前根据 `localStorage` / `prefers-color-scheme` 设置 `html.dark`，防闪烁
- 顶栏增加主题切换按钮（浅色 / 暗色 / 跟随系统），偏好写入 `localStorage`
- `color-scheme` 与表单控件、滚动条跟随主题
- 顺带补齐页面已使用但未定义的 `btn-ghost`、`field-hint`

## Capabilities

### New Capabilities

- `color-theme`: 浅色/暗色主题切换、偏好持久化、系统跟随、首帧防闪烁

### Modified Capabilities

- 无（不改业务行为；视觉主题属新能力）

## Impact

- `AppPortal/Styles/app.css`：`@custom-variant dark`、语义变量、组件层 dark 变体、raw rgb 改造
- `AppPortal/Pages/Shared/_Layout.cshtml`：内联防闪烁脚本、切换按钮、`color-scheme`
- 各业务页 cshtml：工具类补 `dark:`（Index / Detail / Admin/*）
- `Create.cshtml` / `Edit.cshtml` / `Import.cshtml` 内联 JS 模板类名同步
- 构建：`npm run css:build` 产出 `wwwroot/css/app.css`
- 无服务端/数据模型/接口变更；无新 NuGet
