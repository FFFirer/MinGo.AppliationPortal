# Delta: pwa-installable

## MODIFIED Requirements

### Requirement: 最小 Service Worker
门户 SHALL 注册一个最小 Service Worker 以满足可安装条件，且不改变现有网络行为。

#### Scenario: SW 注册成功
- **WHEN** 浏览器支持 Service Worker（安全上下文，或 `chrome://flags` 白名单等已按安全源放行）
- **THEN** 尝试注册 `/sw.js`
- **THEN** SW 含 `fetch` 事件处理器

#### Scenario: 非安全上下文静默失败
- **WHEN** 通过纯 HTTP（非 localhost）访问且浏览器拒绝 SW
- **THEN** 注册失败被静默捕获
- **THEN** 不抛出未捕获错误、不刷屏 console 报错
- **THEN** 页面功能不受影响

## ADDED Requirements

### Requirement: 安装入口与 HTTP 引导
门户 SHALL 提供安装入口；在浏览器不给原生安装提示时（典型为 HTTP 明文部署）SHALL 提供平台化安装引导，而不是留白。

#### Scenario: 原生可安装
- **WHEN** 浏览器触发 `beforeinstallprompt`（HTTPS 或 localhost 等安全上下文）
- **THEN** 顶栏出现「安装」按钮
- **THEN** 点击调起浏览器原生安装流程

#### Scenario: HTTP 明文安装引导
- **WHEN** 站点以 HTTP 非 localhost 访问，无 `beforeinstallprompt`
- **THEN** 顶栏「安装」打开引导弹窗
- **THEN** 引导包含 `chrome://flags/#unsafely-treat-insecure-origin-as-secure` 用法（含完整 origin）
- **THEN** 引导包含 `localhost` 端口转发方式
- **THEN** 引导包含 iOS / Android 主屏添加路径

#### Scenario: 已安装不打扰
- **WHEN** 应用已以 `standalone` / `fullscreen` / `minimal-ui` display-mode 运行
- **THEN** 不显示安装按钮

#### Scenario: 安装完成隐藏
- **WHEN** 浏览器派发 `appinstalled`
- **THEN** 安装按钮隐藏
