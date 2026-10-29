# 项目上下文

## PWA 图标生成方案

**决策背景**：PWA 图标（favicon、apple-touch-icon、manifest icons）需要和应用内显示的火箭图标完全一致。

**问题**：最初用 Pillow 绘制几何火箭，方向（垂直向上）和风格（实心填充）都与 SVG sprite 中的 `#i-rocket` 不一致。

**解决方案**：使用 Node.js sharp 直接渲染 SVG sprite 中的火箭路径数据，确保图标和应用内显示的完全相同。

**技术细节**：
- SVG 路径来自 `Pages/Shared/_IconSprite.cshtml` 中的 `#i-rocket` symbol
- 使用 `sharp` 库将 SVG 渲染成 PNG
- 生成脚本：`tools/generate_icons.js`
- 输出：`wwwroot/icons/icon-192.png`、`icon-512.png`、`apple-touch-icon.png`、`icon-maskable-512.png`、`favicon.ico`
- 圆角由 raw 像素级 4x4 超采样 alpha 实现（`roundRectCoverage`），不要再用 SVG + `dest-in` 合成
- favicon.ico 手写 ICO 容器，内嵌 16/32/48 PNG 帧

**比例基准**：
- 普通图标（192/512/180）火箭比例 0.60，白色轮廓实际约占图块 53%
- `icon-maskable-512.png` 必须用 0.41：Android 自适应图标把 maskable 画布裁到约 66.7% 可视区，内容会被放大约 1.46 倍；0.60 在桌面上会显得过大（实测占桌面图标 78%）
- 修改比例后可用「裁切中间 66.7% 再量白色 bbox」的方式验证 maskable 观感

**验证方法**：启动器图标是安装时固化的，发布新图标后需要删除旧快捷方式重新添加；Chrome 对同源图标有缓存，可清理站点数据后重试。

**依赖**：Node.js + sharp（已安装在 `tools/node_modules/`）

**已知坑**：
- Windows 上 cairosvg 需要系统级 cairo DLL，不可用
- svglib + reportlab 也需要 cairo 后端
- sharp 是跨平台方案，内置 SVG 渲染支持
- 旧实现中 `<svg><rect/></svg>` 无宽高/viewBox 时 `dest-in` 遮罩静默失效，会导致 icon-512 与 maskable 字节完全相同（圆角丢失）
