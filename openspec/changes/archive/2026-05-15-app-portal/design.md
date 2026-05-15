## Context

全新构建一个内部应用门户站点，基于 ASP.NET Core 10 Razor Pages + Tailwind CSS v4 + LiteDB。项目从零开始，无历史遗留代码。目标是为团队提供一个统一的应用入口导航页和管理后台。

## Goals / Non-Goals

**Goals:**
- Razor Pages 服务端渲染首页，展示已启用应用列表
- 管理后台完整 CRUD，含新增/编辑/删除/启用禁用
- 每个应用支持配置多个完整 HTTP URL，点击在新标签页打开
- LiteDB 嵌入式存储，无需外部数据库基础设施
- Tailwind CSS v4 样式，现代响应式 UI
- 数据库路径通过 `appsettings.json` 配置，支持 fallback 默认值 `AppPortal.db`

**Non-Goals:**
- 用户认证和授权（后续可按需添加）
- 应用分类/标签系统
- 访问统计或埋点
- 应用图标上传（仅支持 URL 引用）
- REST API（纯 Razor Pages 页面交互）

## Decisions

### 1. Razor Pages vs MVC vs Blazor
- **选择**: Razor Pages
- **理由**: 这是一个页面为主的应用，每个页面（首页、管理列表、新增、编辑、删除）对应一个 Razor Page，PageModel 即 controller，适合 CRUD 场景。相比 MVC 减少了 controller/action 到 view 的间接层，相比 Blazor 无需 WASM 或 SignalR 开销。
- **替代方案**: MVC 也行，但 Razor Pages 对这种每个页面一个独立 handler 的模式更自然。

### 2. LiteDB 单例注册
- **选择**: `LiteDatabase` 注册为 Singleton
- **理由**: LiteDB 是嵌入式数据库，线程安全（内部使用读写锁），单例避免重复创建连接和文件句柄。`AppService` 注册为 Scoped，每个请求独立实例但共享同一数据库。
- **风险**: LiteDB 5.x 的 `LiteDatabase` 实例是线程安全的，但在高并发写入场景下写锁可能成为瓶颈。门户场景写入频率低（仅后台管理操作），风险可控。

### 3. Tailwind CSS v4 构建集成
- **选择**: MSBuild Target 自动调用 `@tailwindcss/cli` CLI
- **理由**: 开发期运行 `--watch` 模式自动重建 CSS；生产构建集成到 `dotnet build` 管线，无需额外构建步骤。
- **替代方案**: 独立 npm script + 手动运行。集成到 MSBuild 更可靠，避免遗忘构建 CSS。

### 4. 多地址数据模型
- **选择**: `List<AppUrl>` 内嵌文档数组，每个 `AppUrl` 含 `Url` + `DisplayName`
- **理由**: LiteDB 原生支持文档嵌套，一个应用的多地址作为子文档内嵌，查询时一次读取无需 JOIN 或关联查询。地址数量通常极少（1-5 个），性能无影响。
- **替代方案**: 拆分独立 `AppUrl` 表 → 过度设计，增加复杂度。

### 5. 管理页表单动态地址行
- **选择**: Razor Pages list binding + 少量 Vanilla JS 实现动态增减
- **理由**: ASP.NET Core 模型绑定原生支持 `Urls[0].Url`、`Urls[1].Url` 模式，JS 只需操作 DOM 的 `index` 属性即可。无需引入 jQuery 或 React/Vue。
- **限制**: 页面需要维持索引连续性（删除时重排），用 JS 管理隐藏的索引计数器。

### 6. ObjectId URL 路由
- **选择**: 使用 LiteDB 的 `ObjectId` 作为 URL 参数（如 `/Admin/Edit/{id}`）
- **理由**: LiteDB 自动生成 ObjectId，字符串表示（如 `5f1a2b3c4d5e6f7a8b9c0d1e`）在 URL 中安全可用。`ObjectId.TryParse()` 可直接解析路由参数。

## Risks / Trade-offs

| 风险 | 缓和措施 |
|------|---------|
| Tailwind v4 `@source` 指令可能无法正确扫描 `.cshtml` 中的类名，导致样式缺失 | 构建后检查 CSS 输出大小；可切换为 `@config` 指向传统 JS 配置 |
| LiteDB 文件锁：多实例部署时文件被占用 | 当前为单实例部署设计，如有多实例需求可迁移到 SQLite 或其他服务端数据库 |
| ObjectId 在 URL 中不美观且不具可读性 | 后续可改为 GUID 或自增 ID，但 ObjectId 功能无影响 |
| 管理页无认证，任何人都可访问管理后台 | 已确定暂不加，后续可叠加 Cookie Auth 中间件（只需几行代码） |
