# Proposal: app-modules-hosts-import

## Why

应用已从「几个外链」长成「一套带前端/API/后台/中间件的系统」，扁平 `Urls` 无法表达应用内功能边界，卡片塞满链接也不可读。同时团队用 Portainer 管理 stack，手工抄端口成本高、易漏；后续还会从 Compose 文件等渠道导入，需要统一的数据源抽象，而不是绑死 Portainer。

## What Changes

- **卡片瘦身**：首页卡片只展示应用身份（图标、名称、描述），访问链接全部移入详情页
- **详情页 + 应用内模块**：详情页按 `AppModule` 分组展示入口；模块可排序、可描述、可独立启用/禁用
- **多 Host**：全局 Host 目录 + 应用引用；详情页 Host 切换器，入口按当前 Host 解析 URL；无映射入口灰显「当前网络不可达」，整模块全空则折叠
- **数据源抽象**：`DataSource` + `IAppSource` + `AppDraft` 归一化导入；Portainer 为首个实现（stack → 应用，service → 模块，端口独立进各模块入口）
- **BREAKING** 数据模型：`AppUrl` 扁平列表改为 `AppModule[] / AppEndpoint[]`；存量 `Urls` 迁入默认模块
- **BREAKING** 首页不再展示可点击地址链接（`card-url-display` 仅卡片截断场景被详情页取代）

## Capabilities

### New Capabilities

- `app-detail-modules`: 应用详情页，按模块展示入口，支持多 Host 切换与不可达态展示
- `app-hosts`: 全局 Host 目录、应用级 Host 引用、入口级 Host 绑定与 URL 拼装
- `app-import-sources`: 数据源连接管理与导入管道（DataSource / IAppSource / AppDraft），含 Portainer stack 导入与端口按 service 独立识别

### Modified Capabilities

- `app-portal-home`: 卡片仅展示应用身份信息，不再内嵌访问地址链接
- `app-portal-admin`: 表单从动态地址行改为模块 + 入口编辑；应用列表增加来源列
- `app-portal-storage`: 数据模型扩展为 Host / AppModule / AppEndpoint / DataSource，服务接口随之扩展

## Impact

- `AppPortal/Models/*`：新增 `Host`、`AppModule`、`AppEndpoint`、`DataSource` 等模型，`AppEntry` 结构变更
- `AppPortal/Services/*`：扩展 CRUD；新增 `IHostService`、`IDataSourceService`、`IImportService`、`IPortainerClient`
- `AppPortal/Pages/`：新增 `Detail`、`Admin/Sources`、`Admin/Import`；调整 `Index`、`Admin/Create|Edit`
- LiteDB 集合：`apps` 字段结构变更（迁移旧 Urls），新增 `hosts`、`datasources`
- 依赖：新增 `System.Text.Json` 内置；Portainer 通过 `HttpClient` 调用，无新 NuGet 必选包
- 密钥：Portainer URL / API Key 存于 DataSource.Config，需在管理界面配置（不入库到 git）
