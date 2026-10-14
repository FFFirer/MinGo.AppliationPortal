# pwa-installable

## Purpose

使门户可被浏览器识别为可安装的 Web 应用（PWA L1）：正确图标、standalone 启动、主题色，并以最小 Service Worker 满足 Chromium 安装条件。

## Requirements

### Requirement: Web App Manifest
门户 SHALL 提供合法的 `manifest.webmanifest`，使浏览器将站点识别为可安装的 Web 应用。

#### Scenario: Manifest 可被浏览器解析
- **WHEN** 浏览器加载任意门户页面
- **THEN** `<head>` 含 `<link rel="manifest" href="/manifest.webmanifest">`
- **THEN** Manifest 包含 `name`、`short_name`、`id`、`start_url`、`scope`、`display`、`icons` 字段
- **THEN** `display` 为 `standalone`

#### Scenario: 可安装条件满足
- **WHEN** 在安全上下文（HTTPS 或 localhost）打开门户
- **THEN** 浏览器检测到满足安装条件（Manifest + Service Worker）
- **THEN** 出现安装入口（地址栏安装图标或菜单项）

### Requirement: 应用图标
门户 SHALL 提供完整图标套件，安装到主屏幕后显示正确品牌图标。

#### Scenario: 标准图标
- **WHEN** Manifest 声明 192x192 与 512x512 图标
- **THEN** 图标文件真实存在且像素尺寸正确
- **THEN** 图案为品牌 `rocket` 渐变方块视觉

#### Scenario: Maskable 图标
- **WHEN** 启动器按 maskable 规范裁切图标
- **THEN** `icon-maskable-512.png` 主体位于安全区内（约 20% padding）
- **THEN** 裁切后火箭图案完整可辨

#### Scenario: iOS 主屏图标
- **WHEN** 用户在 iOS Safari「添加到主屏幕」
- **THEN** 使用 `apple-touch-icon`（180x180）
- **THEN** 图标显示为品牌方块而非页面截图或默认图

### Requirement: 主题色与 iOS Meta
门户 SHALL 声明 `theme-color` 与 iOS web-app meta，安装后状态栏/启动色与主题一致。

#### Scenario: theme-color 跟随主题
- **WHEN** 用户处于浅色主题
- **THEN** `theme-color` 为 `#2563eb`
- **WHEN** 用户处于暗色主题
- **THEN** `theme-color` 为 `#111827`

#### Scenario: 切换主题即时更新
- **WHEN** 用户通过顶栏切换浅/暗/跟随系统
- **THEN** `theme-color` meta 随之更新，无需刷新

#### Scenario: iOS meta 齐备
- **WHEN** 页面在 iOS Safari 渲染
- **THEN** 存在 `apple-mobile-web-app-capable` 与 `apple-mobile-web-app-status-bar-style` meta

### Requirement: 最小 Service Worker
门户 SHALL 注册一个最小 Service Worker 以满足可安装条件，且不改变现有网络行为。

#### Scenario: SW 注册成功
- **WHEN** 页面运行在安全上下文
- **THEN** 注册 `/sw.js`
- **THEN** SW 含 `fetch` 事件处理器

#### Scenario: 不缓存任何请求
- **WHEN** SW 已激活后浏览任意页面
- **THEN** 所有请求直达网络，无 Cache Storage 写入
- **THEN** 响应与未注册 SW 时一致

#### Scenario: 非安全上下文静默跳过
- **WHEN** 通过纯 HTTP（非 localhost）访问
- **THEN** 不注册 SW
- **THEN** 不抛出未捕获错误、不刷屏 console 报错

#### Scenario: 注册失败静默
- **WHEN** SW 注册因任何原因失败
- **THEN** 页面功能完全不受影响
