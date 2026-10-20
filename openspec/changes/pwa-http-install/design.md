# Design: pwa-http-install

## Context

已具备 L1 PWA：`manifest.webmanifest`、图标套件、最小 `sw.js`、theme-color。SW 注册被 `window.isSecureContext` 拦住，HTTP 非 localhost 下静默不注册，Chromium 亦不给安装入口。

部署现实：Docker `ASPNETCORE_URLS=http://+:8080`，内网 IP/域名直连或反代 HTTP 终结，即多数访问是「非安全上下文」。

## Goals / Non-Goals

**Goals:**

- HTTP 明文下用户能完成「加到主屏 / 桌面」——通过浏览器菜单快捷方式或 flags 白名单后的原生安装
- HTTPS / localhost 下一键原生安装，体验不回退
- 已安装（standalone）不再打扰
- 失败静默，不污染 console

**Non-Goals:**

- 不伪造 secure context，不注入扩展/策略，不改浏览器安全模型
- 不做自签证书 / 内网 CA 自动签发
- 不做 L2 离线；不改 manifest 语义

## Decisions

1. **SW：`serviceWorker` 存在即尝试注册，去掉 `isSecureContext` 门闩**  
   HTTP 非 localhost 下 `register()` 会抛 `SecurityError`，`catch` 吞掉。  
   收益：`chrome://flags/#unsafely-treat-insecure-origin-as-secure` 白名单源、以及未来浏览器策略松动时无需改代码。  
   备选：保留 isSecureContext 门闩 —— flags 白名单后页面已是 secure context，门闩反而无害；但对「API 在却 register 失败」的环境不透明，统一 try/catch 更干净。

2. **安装入口分两档：原生 prompt 优先，引导弹窗兜底**  
   - 监听 `beforeinstallprompt`（`preventDefault` 存 deferredPrompt）→ 顶栏「安装」直接 `prompt()`  
   - 无 deferredPrompt 时点「安装」→ 引导弹窗  
   - `matchMedia('(display-mode: standalone)')` 或 iOS `navigator.standalone` 为真 → 隐藏按钮  
   备选：只在可安装时显示按钮 —— HTTP 下用户永远看不到入口，违背本次目标。

3. **引导弹窗内容按检测结果排序，不写死长文**  
   优先级：当前 origin 是否 secure → 平台（iOS / Android / Desktop）→ 给 2～3 条可操作步骤。  
   HTTP 非 localhost 必出：
   - `chrome://flags/#unsafely-treat-insecure-origin-as-secure` 粘贴完整 origin，启用后重启，再点安装  
   - 或用 SSH/Docker 端口转发到 `http://localhost:xxxx` 再装  
   - iOS/Android 主屏菜单手动添加（图标已就绪）  
   备选：外链详细文档 —— 弹窗内闭环更好用。

4. **UI 紧贴现有设计系统**  
   顶栏按钮复用 `.theme-toggle` 气质 + `#i-download`；弹窗复用 Host 模态框结构（固定头尾、遮罩、Esc、`aria-hidden`）。不新增样式体系。

## 验收口径

- HTTPS/localhost：`beforeinstallprompt` 触发后顶栏「安装」可完成安装；安装后按钮消失
- HTTP IP 访问：顶栏「安装」打开引导；Console 无未捕获异常；Network 中 SW 未注册（符合预期）
- iOS Safari：引导文案含「分享 → 添加到主屏幕」
- standalone 启动：无安装按钮
