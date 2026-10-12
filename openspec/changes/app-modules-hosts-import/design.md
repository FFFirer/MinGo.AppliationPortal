## Context

AppPortal 是内部应用门户（ASP.NET Core 10 Razor Pages + LiteDB）。现状：`AppEntry.Urls` 为扁平 `AppUrl{Url,DisplayName}`，首页卡片直接列出链接，无 stack/模块/Host 概念。团队用 Portainer 管理 Compose stack，需要把 stack 元数据导入成应用，且不同 service 的端口能独立识别。

调研结论（Homarr / Homepage / Dashy）：卡片做身份、详情承载链接更清晰；多 Host 用显式切换（Homepage instance 模式）优于浏览器探测（Dashy localUrl）；应用内子入口需要一等公民「模块」，不能只是图标级 subItems。

## Goals / Non-Goals

**Goals:**
- 卡片只展示应用身份；详情页按模块展示入口
- 全局 Host 目录 + 应用 Host 引用 + 详情切换器；URL 由 Host+Port+Path 拼装，可 UrlOverride
- 模块可独立 IsEnabled；Host 无映射入口灰显，整模块不可达则折叠
- 数据源抽象（DataSource / IAppSource / AppDraft），Portainer 为首个 source
- 旧 `Urls` 自动迁入默认模块，保证升级后数据不丢

**Non-Goals:**
- 按用户/组的模块 RBAC（后续）
- 自动探测偏好 LAN 并切换 Host（后续可选）
- 实时健康检查 / Ping（Homarr/Dashy 的 statusCheck）
- Portainer 以外的 source 实现（只留扩展点）
- 卡片上的快捷直达链接

## Decisions

### D1. 模块（AppModule）而非分组（LinkGroup）
- **选择**：`AppEntry.Modules[]`，每模块含 Name/Description/IsEnabled/Order/Endpoints[]/SourceService?
- **理由**：模块是应用功能单元（前台/API/数据），服务端 `SourceService` 只作导入溯源；比「展示分组」更符合产品语义，也便于独立启停
- **备选**：扁平 Urls + GroupName 字段——迁移简单但无法表达模块描述/启停/排序

### D2. 多 Host：全局目录 + 应用引用 + 入口覆盖
- **选择**：`Host` 全局表；`AppEntry.HostRefs[]`；`AppEndpoint.HostId?` + Port/Path/Scheme/UrlOverride
- **展示**：详情页 Host 切换器；当前 Host 下每入口一条解析 URL；无映射灰显「当前网络不可达」；模块全空则折叠
- **备选 A**：每入口展开全 Host 列表——模块×Host 笛卡尔积爆炸
- **备选 B**：浏览器探测 localUrl——多入口慢且受 CORS 限制

### D3. URL 拼装
```
Http:  scheme://host.Address:Port/Path
Tcp/Udp: host.Address:Port（纯文本，不生成 <a>）
UrlOverride 优先于拼装
SchemeOverride 优先于 Host.SchemeDefault
Port 为空且 scheme=https → 443；http → 80（可省略端口）
```

### D4. 数据源管道
```
DataSource (Type, Config JSON)
    → IAppSource.ListCandidates / Resolve
    → AppDraft { Name, Modules[{Name, Endpoints[], IsEnabled?}], HostHints[] }
    → 导入预览（勾选/改名/合并）
    → AppEntry + SourceId/ExternalId
```
- Portainer：Stack→App，`com.docker.compose.service`→Module，`Ports`→Endpoint（HostPort/ContainerPort/Kind）
- 再同步默认：仅提示漂移（SyncHint），不自动覆盖

### D5. 迁移
- 读到无 `Modules` 但有旧 `Urls` 的文档 → 合成模块「访问入口」，每条 Url → Http Endpoint（UrlOverride=原 Url）
- 不删旧字段（LiteDB 无强制 schema），写入后以新结构为准

### D6. 技术落点
- `IPortainerClient`：HttpClient + API Key/JWT；`GET /api/stacks`、`GET /api/stacks/{id}/file`、`GET /api/endpoints/{id}/docker/containers/json`
- 服务注册：Singleton LiteDatabase；Scoped 各 Service；HttpClient 工厂
- 页面：`/Detail/{id}`、`/Admin/Sources`、`/Admin/Import`

## Risks / Trade-offs

- [LiteDB 文档结构变更] → 启动时/读取时惰性迁移旧 Urls；保留兼容字段
- [Portainer API 版本差异] → 只依赖 stacks/containers 基本字段；失败在预览页报错，不写半截数据
- [Host 配置错误导致坏链] → 导入预览展示拼装结果；UrlOverride 可救
- [模块/入口表单复杂度上升] → 分区表单：模块块内嵌套入口行；先满足 CRUD，交互后置打磨
- [端口非 HTTP 误生成链接] → Kind=Tcp/Udp 不渲染 <a>，只显示 host:port

## Migration Plan

1. 部署新版本后首次读取 `apps` 自动迁移 Urls→Modules
2. 管理员在 `/Admin/Hosts`（或 Sources 配置内）补全 Host 目录，并为已有应用绑定 Host
3. 可选：配置 Portainer DataSource，导入或校对已有应用
4. 回滚：旧版本读不懂 Modules 字段，但不崩溃（忽略未知字段）；关键数据在旧 Urls 仍在

## Open Questions

- Host 切换偏好是否进 localStorage（实现时按 localStorage，不进库）
- Portainer 认证先支持 API Key，JWT 登录是否本期（实现先 API Key）
