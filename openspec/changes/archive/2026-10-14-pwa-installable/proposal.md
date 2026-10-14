# Proposal: pwa-installable

## Why

门户定位是「应用启动器」，用户希望像 App 一样添加到手机/桌面主屏幕，随手一点打开。当前项目 **完全没有 PWA 能力**：

- 无 `manifest.webmanifest`，浏览器无法识别为可安装应用
- 仅有 5KB 的 `favicon.ico`，没有 192/512/maskable 图标，加到主屏是系统默认丑图
- 无 `theme-color` / `apple-touch-icon`，iOS 与安卓沉浸式状态栏体验缺失
- 无 service worker，Chromium 安装条件不满足，不会弹出「安装应用」

目标只做 **L1：可安装**。不做离线缓存、不做推送、不做数据缓存策略（L2+）。

## What Changes

- 新增 `wwwroot/manifest.webmanifest`：`name` / `short_name` / `id` / `start_url` / `scope` / `display: standalone` / `theme_color` / `background_color` / `icons`（192、512、maskable 512）
- 新增 PWA 图标套件：从顶栏 `rocket` 渐变方块品牌视觉提炼，产出 `icon-192.png`、`icon-512.png`、`icon-maskable-512.png`、`apple-touch-icon.png`（180）
- 新增 `wwwroot/sw.js`：**仅满足可安装条件的最小 SW**（`fetch` 事件空实现），**不做任何缓存**，不改变现有网络行为
- `_Layout.cshtml` `<head>` 补齐：`<link rel="manifest">`、`theme-color`（跟随当前浅/暗主题色值）、`apple-touch-icon`、iOS web-app meta
- `_Layout.cshtml` 尾部注册 service worker（仅 https / 非 localhost 条件下注册，注册失败静默）
- 无需服务端、数据模型、路由变更；无新 NuGet；无 npm 依赖变更

## Capabilities

### New Capabilities

- `pwa-installable`: Web App Manifest、图标套件、theme-color、最小 Service Worker、主屏安装体验

### Modified Capabilities

- `color-theme`: `theme-color` meta 与主题切换联动（浅色/暗色两套状态栏色），属于主题能力的延伸，不改切换行为本身

## Impact

- `AppPortal/wwwroot/manifest.webmanifest`：新建
- `AppPortal/wwwroot/icons/`：新建图标目录（4 个 png）
- `AppPortal/wwwroot/sw.js`：新建（最小 SW）
- `AppPortal/Pages/Shared/_Layout.cshtml`：head 标签与 SW 注册脚本
- 构建：`npm run css:build` 不受影响（纯静态资源 + 布局改动）
- 部署：需 HTTPS（或 localhost）才能触发安装；HTTP 明文部署无法安装（部署约束，不是代码改动）
- **明确不做**：离线页、静态资源缓存、网络优先壳、数据缓存、Web Push、后台同步

## 风险与约束

| 风险 | 说明 | 应对 |
|---|---|---|
| 多 Host / 多 origin | SW 与安装态按 origin 隔离，换域名 = 重新安装 | 可接受；各 Host 独立安装 |
| HTTP 部署无法安装 | 安装条件要求安全上下文 | 部署侧保证 HTTPS；开发 localhost 可测 |
| 最小 SW 的未来扩展 | 现在空实现，L2 再加缓存策略会触发 SW 更新流程 | 文件名固定 `sw.js`，后续版本递增 `CACHE_VERSION` 即可 |
| 图标 maskable 安全区 | 不按 maskable 规范会被启动器裁切变丑 | 产出时预留 safe zone（约 20% padding） |
| iOS 不读 manifest | iOS 靠 `apple-touch-icon` + meta | 同步产出 180px 图标与 apple meta |
