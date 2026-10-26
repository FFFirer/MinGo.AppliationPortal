# Tasks: portainer-project-env

## 1. PortainerEnvironment 实体与服务

- [x] 1.1 新增 `Models/PortainerEnvironment.cs`：Id, DataSourceId, EndpointId, Name, Type, HostIds, LastSyncAt
- [x] 1.2 新增 `Services/IEnvironmentService.cs`：GetByDataSourceAsync, GetByIdAsync, SyncFromPortainerAsync, UpdateHostsAsync, DeleteAsync
- [x] 1.3 新增 `Services/EnvironmentService.cs`：LiteDB `environments` 集合，实现 CRUD + 同步逻辑
- [x] 1.4 `Program.cs` 注册 `IEnvironmentService` 为 Scoped

## 2. PortainerClient API 改造

- [x] 2.1 新增 `PortainerEndpointInfo` / `PortainerProjectInfo` record
- [x] 2.2 实现 `ListEndpointsAsync`：`GET /api/endpoints` → 解析 Id/Name/Type
- [x] 2.3 实现 `ListProjectsAsync`：`GET /api/endpoints/{id}/docker/containers/json` → 按 `com.docker.compose.project` 分组 → 返回 project 列表
- [x] 2.4 实现 `GetProjectPortsAsync`：复用容器查询，按 project name 过滤
- [x] 2.5 保留 `ListStacksAsync` / `GetStackPortsAsync` 底层封装不删除（供存量迁移使用），但 `PortainerAppSource` 不再调用它们

## 3. DataSource 模型精简

- [x] 3.1 `DataSource.cs` 移除 `HostIds` / `LastSyncAt` / `LastStatus` 字段
- [x] 3.2 `DataSourceService.cs` 移除 Host 绑定相关逻辑
- [x] 3.3 `DataSourceService.TestConnectionAsync` 保持不变（仅测试连通性）

## 4. AppEntry 模型调整

- [x] 4.1 `AppEntry.cs` 新增 `ObjectId? EnvironmentId` 字段
- [x] 4.2 幂等匹配继续在 `ImportService` 内按 `(SourceId, EnvironmentId, ExternalId)` 过滤，不新增专用查询方法

## 5. IAppSource / PortainerAppSource 适配

- [x] 5.1 `IAppSource` 接口：`ListCandidatesAsync` / `ResolveAsync` 增加 `int endpointId` 参数
- [x] 5.2 `PortainerAppSource.ListCandidatesAsync`：调用 `ListProjectsAsync(configJson, endpointId)`
- [x] 5.3 `PortainerAppSource.ResolveAsync`：调用 `GetProjectPortsAsync(configJson, endpointId, projectName)`；`ExternalCandidate.ExternalId` = project name
- [x] 5.4 `ImportService`：传递 endpointId 到 IAppSource；幂等匹配键增加 EnvironmentId

## 6. Sources 页面 Environment 展开区域

- [x] 6.1 `Sources.cshtml.cs`：加载每个 DataSource 的 Environment 列表；新增 Sync / UpdateEnvironmentHosts handler
- [x] 6.2 `Sources.cshtml`：每个数据源行下方嵌入可展开区域，展示 Environment 列表（Name, Type, EndpointId, Host 绑定状态）
- [x] 6.3 每个 Environment 行增加「绑定 Host」按钮 + 模态框
- [x] 6.4 每个 Environment 行增加「导入」按钮，跳转到 `/Admin/Import/{sourceId}/{environmentId}`
- [x] 6.5 增加「同步」按钮，手动触发从 Portainer 拉取最新 endpoints；进入页面时静默自动同步
- [x] 6.6 移除 DataSource 行的「绑定 Host」按钮和相关模态框
- [x] 6.7 移除创建表单中的 Host 绑定多选区域

## 7. Import 页面适配

- [x] 7.1 路由改为 `/Admin/Import/{sourceId}/{environmentId?}`（environmentId 可选，缺省回落第一个 Environment）
- [x] 7.2 `Import.cshtml.cs`：从 Environment.HostIds 加载 Host 候选池
- [x] 7.3 候选列表展示 compose projects
- [x] 7.4 旧路由 `/Admin/Import/{sourceId}` 自动回落到第一个 Environment（兼容书签）

## 8. 应用删除去掉禁用前置条件

- [x] 8.1 `Delete.cshtml.cs`：移除 `OnGetAsync` 和 `OnPostAsync` 中的 `IsEnabled` 检查
- [x] 8.2 `Delete.cshtml`：移除"仅已禁用的应用可以删除"警告文案
- [x] 8.3 `Admin/Index.cshtml`：删除按钮移出 `@if (!app.IsEnabled)` 条件，始终渲染

## 9. 存量数据自动迁移

- [x] 9.1 新增 `Services/DataMigration.cs`：启动时检测并执行一次性迁移
- [x] 9.2 DataSource → Environment 迁移：同步 endpoints；旧 `HostIds` 复制到该数据源下所有 Environment
- [x] 9.3 AppEntry.ExternalId 迁移：对旧 stack Id（数字），调用保留的 `ListStacksAsync` 查 stack Name，更新为 project name；设置 EnvironmentId
- [x] 9.4 `Program.cs` 启动时调用迁移方法
- [x] 9.5 迁移失败记录日志但不阻断启动，保留标记下次重试

## 10. 验证

- [x] 10.2 空库启动无报错（`LiteDb__Path` 指向临时库，首页与管理页均 200）
- [x] 10.6 应用删除：启用状态的应用可直接删除（表单实测 302 + 记录移除）
- [ ] 10.1 存量迁移：有旧数据时启动，验证 Environment 创建、HostIds 搬迁、ExternalId 映射正确（需可达 Portainer）
- [ ] 10.3 Sources 页面：展开区域与 Host 绑定交互（需浏览器实测）
- [ ] 10.4 Import 流程：选 Environment → 列 compose projects → 预览 → 改写 URL → 确认导入（需可达 Portainer）
- [ ] 10.5 原生 compose 项目（非 Portainer 管理的 stack）能被正确发现和导入（需可达 Portainer）
- [ ] 10.7 幂等导入：同 (SourceId, EnvironmentId, ExternalId) 再次导入更新而非新建（需可达 Portainer）
- [ ] 10.8 旧路由 `/Admin/Import/{sourceId}` 回落第一个 Environment（需有 Environment 数据）
