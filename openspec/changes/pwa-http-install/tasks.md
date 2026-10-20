# Tasks: pwa-http-install

## 1. Service Worker 注册策略

- [x] 1.1 `_Layout.cshtml`：SW 注册改为存在 `serviceWorker` 即 `register('/sw.js')`，失败静默（去掉 `isSecureContext` 硬门闩）

## 2. 安装按钮与引导

- [x] 2.1 新建 `wwwroot/js/app-install.js`：deferredPrompt、standalone 检测、平台探测、安装/引导逻辑
- [x] 2.2 `_Layout.cshtml` 顶栏增加「安装」按钮（默认隐藏，脚本控制显示）
- [x] 2.3 增加安装引导弹窗（复用 Host 模态框结构），含 Chrome/iOS/Android/HTTP flags/localhost 隧道指引
- [x] 2.4 standalone 模式隐藏安装入口；`appinstalled` 后隐藏

## 3. 规格同步

- [x] 3.1 更新 `openspec/specs/pwa-installable/spec.md`：HTTP 安装引导、SW 尝试注册、standalone 隐藏

## 4. 验证

- [x] 4.1 localhost：页面含安装按钮/弹窗/脚本指纹引用；SW 仍注册（安全上下文）
- [x] 4.2 非 secure context 策略：SW 失败走 catch 静默；安装按钮打开引导（含 flags / localhost / 主屏）
- [x] 4.3 standalone 判定逻辑覆盖 display-mode 与 iOS `navigator.standalone`
