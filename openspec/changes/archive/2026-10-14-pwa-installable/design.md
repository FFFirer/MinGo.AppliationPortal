# Design: pwa-installable

## Context

技术栈：ASP.NET Core Razor Pages + Tailwind CSS v4 + LiteDB。布局统一走 `Pages/Shared/_Layout.cshtml`，静态资源在 `wwwroot/`，现状仅有 `css/app.css` 与 `favicon.ico`。

品牌视觉：顶栏 logo 为 `w-9 h-9 rounded-xl bg-gradient-to-br from-blue-600 to-indigo-600` + `#i-rocket` 白色火箭图标。主题已支持 light/dark/system（`color-theme`）。

目标：**L1 可安装**——浏览器识别为应用、提供安装入口、主屏图标/名称正确、standalone 全屏体验。不碰离线与缓存。

## Goals / Non-Goals

**Goals:**

- Chromium / Android Chrome / 桌面 Chrome 可弹出「安装应用」并安装
- iOS Safari 可「添加到主屏幕」，图标与名称正确
- 安装后 standalone 启动，自定义图标、启动色、状态栏主题色
- `theme-color` 随浅/暗主题切换
- 最小 SW 满足安装条件，**零缓存、零行为改变**

**Non-Goals:**

- 离线可用 / 离线兜底页 / 静态资源 cache
- 网络优先壳、数据缓存策略（L2）
- Web Push、后台同步、Periodic Background Sync
- 图标生成流水线 / 自动从 favicon 派生
- standalone 模式专用导航（返回条、手势）
- 安装引导 UI（beforeinstallprompt 横幅 / 自定义按钮）

## Decisions

1. **最小 SW，仅带 `fetch` 空实现，不做缓存**  
   Chromium 安装条件要求注册的 service worker 具备 `fetch` 事件处理器。提供一个 `self.addEventListener('fetch', function () { /* 透传，不拦截 */ })` 的最小实现即可满足条件。  
   备选 A：完全不放 SW —— 新版 Chrome 有放松趋势，但各版本/引擎条件不一致，安装入口会时有时无，放弃。  
   备选 B：SW 顺手缓存静态资源 —— 就是 L2，本次明确不做，缓存策略要单独拍板（缓存错 Admin 是事故）。  
   注册条件：仅 `window.isSecureContext` 为真时注册；失败（如 HTTP）静默跳过，不污染控制台。

2. **图标从既有 `rocket` 渐变方块品牌提炼，不引入图标字体/生成服务**  
   产出 4 个 PNG：`icon-192.png`、`icon-512.png`、`icon-maskable-512.png`、`apple-touch-icon.png`(180)。  
   maskable 版本按 Android 规范在四周预留约 20% 安全区，主体缩小居中。  
   备选：继续用 favicon.ico 缩放 —— ico 多尺寸失真且无 maskable，主屏效果差，放弃。  
   资产为静态文件进 `wwwroot/icons/`，不走构建生成。

3. **`manifest` 字段选择**  
   ```
   id:          "app-portal"          ← 与 start_url 解耦，换路径不掉安装
   name:        "应用门户"
   short_name:  "门户"                 ← 主屏显示名，控制字数
   start_url:   "/"
   scope:       "/"
   display:     "standalone"
   theme_color: 跟随主题（见决策 4）
   background_color: "#f6f7fb"         ← 启动闪屏底色，对齐 body 背景
   lang:        "zh-CN"
   icons:       192 / 512 / 512-maskable，purpose 各标 `any` / `maskable`
   ```
   备选 `display: "minimal-ui"`：仍带浏览器返回条，standalone 才像 App。`display: "browser"` 则不可安装。

4. **`theme-color` 与 color-theme 联动，写死一个值会违和**  
   浅色 `#2563eb`（blue-600，对齐品牌蓝顶栏气泡）、暗色 `#111827`（gray-900）。由布局内主题脚本在 `apply()` 时同步写 `<meta name="theme-color">`。  
   备选：固定单一色 —— 暗色下状态栏仍是亮蓝，割裂。  
   此为对 `color-theme` 能力的小幅扩展，不改切换逻辑。

5. **iOS 兼容走 apple 系 meta，不指望 manifest**  
   补 `apple-touch-icon` 与 `apple-mobile-web-app-capable` / `apple-mobile-web-app-status-bar-style`。iOS 无安装提示，用户手动「添加到主屏幕」，图标质量决定第一印象。

6. **不注册 beforeinstallprompt 引导 UI**  
   浏览器自带安装入口（地址栏安装图标 / 菜单）已够用。自定义引导是独立体验问题，留给后续。

## 验收口径

- Chrome DevTools → Application → Manifest：无红字错误，installability 通过
- Chrome 地址栏出现安装图标，安装后主屏/桌面出现正确图标与「应用门户」名
- 安装后 standalone 启动，无浏览器地址栏，顶栏主题色正确
- iOS Safari「添加到主屏幕」后图标为 rocket 方块而非默认截图
- 切换浅/暗主题后 `theme-color` meta 同步变化
- Network 面板：SW 已注册但请求仍直达网络（无 cache hit，行为与改前一致）
