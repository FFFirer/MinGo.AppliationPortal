# Proposal: pwa-http-install

## Why

门户常以 **HTTP 明文 + 内网 IP/域名** 部署（容器只听 `http://+:8080`，HTTPS 在反代）。Chromium 在非 localhost 的 HTTP 下判定为非安全上下文：Service Worker 被拒、`beforeinstallprompt` 不触发，原生「安装应用」入口不出现。用户体感是「发了镜像也装不了」。

浏览器安全策略无法从页面侧绕过 SW/安装对 secure context 的硬依赖。本变更在 **不伪造安全上下文** 的前提下，把 HTTP 部署下「可安装」的路径推到极限，并给出明确引导。

## What Changes

- SW 注册去掉 `isSecureContext` 硬门闩，改为存在 `serviceWorker` 即尝试注册、失败静默（覆盖 localhost 与 `chrome://flags` 白名单源）
- 顶栏增加「安装」按钮：
  - 有 `beforeinstallprompt` 时一键调起原生安装
  - 无原生提示（典型 HTTP）时打开安装引导弹窗
- 安装引导弹窗按平台给出路径：Chrome/Edge 安装、iOS 添加到主屏幕、Android 菜单安装、HTTP 局域网 `chrome://flags` 白名单、`localhost` 端口转发
- 已处于 standalone/全屏 display-mode 时不显示安装入口
- 不改 manifest 字段、图标、主题色；不做 L2 离线缓存

## Capabilities

### New Capabilities

- 无（扩展既有能力）

### Modified Capabilities

- `pwa-installable`: HTTP 明文下的安装引导与 SW 注册策略；明确浏览器硬限制

## Impact

- `AppPortal/Pages/Shared/_Layout.cshtml`：安装按钮 + 引导弹窗 + 事件绑定
- `AppPortal/wwwroot/js/app-install.js`：新建安装逻辑
- `openspec/specs/pwa-installable/spec.md`：同步新要求
- 无服务端/构建/镜像流程变更

## 无法承诺的（浏览器硬限制）

| 场景 | 原生安装弹窗 | 本变更提供的 |
|---|---|---|
| HTTPS 或 localhost | ✅ | 一键安装按钮 |
| HTTP + 非 localhost（默认内网） | ❌ SW/安装 API 不可用 | 安装引导（flags / 主屏手动添加 / localhost 隧道） |
| iOS Safari 任意协议 | 无 PWA 安装 API | 「添加到主屏幕」引导（apple-touch-icon 已具备） |
