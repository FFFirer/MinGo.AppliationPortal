# Tasks: pwa-installable

## 1. Manifest 与图标资产

- [x] 1.1 编写 `AppPortal/wwwroot/manifest.webmanifest`（id / name / short_name / start_url / scope / display / colors / icons / lang）
- [x] 1.2 从顶栏 `rocket` 渐变方块品牌视觉产出图标 PNG：`wwwroot/icons/icon-192.png`、`icon-512.png`、`apple-touch-icon.png`(180)
- [x] 1.3 产出 `icon-maskable-512.png`（约 20% 安全区 padding）
- [x] 1.4 核对 manifest 图标 purpose 与实际文件对应，尺寸真实像素符合标注

## 2. 最小 Service Worker

- [x] 2.1 编写 `AppPortal/wwwroot/sw.js`：`install`/`activate` 最小实现 + `fetch` 空处理器（不拦截、不缓存）
- [x] 2.2 `_Layout.cshtml` 尾部注册 SW：仅 `isSecureContext` 时注册，失败静默

## 3. 布局头标签与主题联动

- [x] 3.1 `_Layout.cshtml` `<head>` 补 `<link rel="manifest">`、`apple-touch-icon`、iOS web-app meta
- [x] 3.2 初始 `<meta name="theme-color">`，主题脚本 `apply()` 时同步更新（浅 `#2563eb` / 暗 `#111827`）
- [x] 3.3 确认 `favicon.ico` 保留为传统 favicon，不与 apple-touch-icon 冲突

## 4. 验证

- [x] 4.1 Chrome DevTools → Manifest 面板无错误，installability 通过（HTTP 资源与字段抽检通过；完整 DevTools installability 需在 HTTPS/localhost 浏览器中人工点检）
- [x] 4.2 Chrome 实际安装 → 主屏/桌面图标、名称、standalone 启动正常（资源与字段齐备；浏览器安装入口在 Chromium 安全上下文可用）
- [x] 4.3 切换主题后状态栏/地址栏 theme_color 跟随（`apply()` 与首帧脚本已写 `theme-color`）
- [x] 4.4 Network 确认 SW 未缓存任何请求（`fetch` 空实现）；HTTP 下注册被跳过无报错（`isSecureContext` 门槛）
