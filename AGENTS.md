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

**依赖**：Node.js + sharp（已安装在 `tools/node_modules/`）

**已知坑**：
- Windows 上 cairosvg 需要系统级 cairo DLL，不可用
- svglib + reportlab 也需要 cairo 后端
- sharp 是跨平台方案，内置 SVG 渲染支持
