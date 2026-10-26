# Design: portainer-project-env

## Context

### 当前架构

```
DataSource (Portainer 实例)
  ├── Config (BaseUrl, ApiKey)
  ├── HostIds → AccessHost[*]    ← Host 绑定在此
  ├── LastSyncAt / LastStatus    ← 同步状态在此
  └── AppEntry (via SourceId)
        ├── ExternalId = stack 数字 Id  ← 只能发现 Portainer 管理的 stack
        └── 无 EnvironmentId
```

导入流程：DataSource → `/api/stacks` → 列 stack → 选 stack → 按 stack.EndpointId 查容器 → 提取端口

### 目标架构

```
DataSource (Portainer 实例)
  └── PortainerEnvironment[*] (自动同步自 /api/endpoints)
        ├── EndpointId (Portainer 数字 Id)
        ├── Name / Type
        ├── HostIds → AccessHost[*]    ← Host 绑定下沉到 Environment
        └── AppEntry (via EnvironmentId)
              └── ExternalId = compose project name  ← 统一发现原生 compose
```

导入流程：DataSource → 选 Environment → `/api/endpoints/{id}/docker/containers/json` → 按 `com.docker.compose.project` 分组 → 列 project → 选 project → 提取端口

## Goals / Non-Goals

**Goals:**

- 导入维度从 Portainer Stack 改为 Compose Project，支持原生 `docker compose up` 部署
- 新增 PortainerEnvironment 实体，自动从 Portainer 同步 endpoints
- Host 绑定从 DataSource 下沉到 Environment
- 应用删除去掉"仅已禁用可删除"前置条件
- 存量数据自动迁移（DataSource.HostIds → 默认 Environment；stack Id → project name）

**Non-Goals:**

- 不改 AppEntry.IsEnabled / AppModule.IsEnabled（禁用/启用开关保留）
- 不改首页只展示启用应用的逻辑
- 不新增独立 Environment 管理页面（嵌入 Sources 展开区域）
- 不改 AccessHost 实体本身
- 不处理 ComposeFile / Manual 类型数据源（仅 Portainer 类型受影响）

## Decisions

### D1. Compose Project 发现方式：查容器按 label 分组

**选择**：调用 `/api/endpoints/{endpointId}/docker/containers/json?all=true`，按 `com.docker.compose.project` label 分组提取唯一 project 列表。

**理由**：
- Portainer 管理的 stack 和原生 `docker compose up` 的容器都带此 label
- 复用现有 `GetStackPortsAsync` 的容器查询逻辑，只是不再按 stack 过滤
- 不依赖 `/api/stacks`，Portainer CE/BE 版本差异不影响

**备选**：先查 `/api/stacks` 再查 `/api/endpoints/{id}/docker/...` 合并——复杂且遗漏原生 compose。

### D2. PortainerEnvironment 实体设计

```csharp
public class PortainerEnvironment
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public ObjectId DataSourceId { get; set; }
    public int EndpointId { get; set; }              // Portainer Endpoint 数字 Id
    public string Name { get; set; } = string.Empty;  // 展示名（同步自 Portainer）
    public string Type { get; set; } = string.Empty;  // "docker-agent" / "docker-standalone" 等
    public List<ObjectId> HostIds { get; set; } = new();
    public DateTime? LastSyncAt { get; set; }
}
```

**EndpointId 类型**：保留 `int`，与 Portainer API 一致。
**Type 字段**：从 Portainer endpoint 的 `Type` 映射（1=docker, 2=agent, 3=azure, 5=kubernetes 等），仅做展示。

### D3. DataSource 模型精简

移除字段：
- `HostIds` → 下沉到 PortainerEnvironment
- `LastSyncAt` / `LastStatus` → 下沉到 PortainerEnvironment

保留字段：
- `Id` / `Name` / `Type` / `Config` / `IsEnabled`

### D4. AppEntry 新增 EnvironmentId

```csharp
public ObjectId? EnvironmentId { get; set; }  // null = 手工创建（非导入）
```

幂等匹配键从 `(SourceId, ExternalId)` 改为 `(SourceId, EnvironmentId, ExternalId)`。

ExternalId 语义变更：从 stack 数字 Id 字符串 → compose project name（字符串）。

### D5. 应用删除：去掉禁用前置条件

**选择**：任何应用均可直接删除，不再要求先禁用。

**理由**：
- 清理重复应用时不必先禁用再删除，减少操作步骤
- 删除确认页本身已有二次确认，足够安全

**影响**：
- `Delete.cshtml.cs`：移除 `OnGetAsync` 和 `OnPostAsync` 中的 `IsEnabled` 检查
- `Delete.cshtml`：移除"仅已禁用的应用可以删除"警告
- `Admin/Index.cshtml`：删除按钮不再受 `@if (!app.IsEnabled)` 限制

### D6. Environment 管理 UI：嵌入 Sources 展开区域

**选择**：在 Sources 页面每个 Portainer 数据源行下方嵌入展开区域，展示该数据源下的所有 Environment。

展开区域内容：
- Environment 列表（Name、Type、EndpointId、Host 绑定状态）
- 每个 Environment 的「绑定 Host」按钮（复用现有模态框样式）
- 「同步」按钮（手动触发从 Portainer 拉取最新 endpoints）
- 「导入」按钮（跳转到 `/Admin/Import/{sourceId}/{environmentId}`）

**不采用独立页面**：减少导航层级，Environment 与 DataSource 的从属关系更直观。

### D7. Import 路由变更

从 `/Admin/Import/{sourceId}` 改为 `/Admin/Import/{sourceId}/{environmentId}`。

Import 页面：
- Host 候选池从 Environment.HostIds 过滤（原逻辑从 DataSource.HostIds）
- 候选列表从 compose projects 获取（原逻辑从 stacks）
- 其余 UI 不变

### D8. 存量数据自动迁移

启动时执行一次性迁移（使用保留的底层 stack API 封装）：

1. **DataSource → Environment**：为每个 `Type == Portainer` 的 DataSource，调用 `/api/endpoints` 拉取所有 endpoints，创建对应 PortainerEnvironment 记录；把 DataSource.HostIds 搬到对应 Environment（按 stack 的 EndpointId 匹配分配）
2. **AppEntry.ExternalId 映射**：对每个有 SourceId 的 AppEntry，若 ExternalId 是数字（旧 stack Id），调用保留的 `ListStacksAsync` 查该 stack 的 Name，将 ExternalId 更新为 project name；同时设置 EnvironmentId 为对应 Environment 的 Id
3. **DataSource 字段清理**：移除 HostIds/LastSyncAt/LastStatus（LiteDB schemaless，新代码不读即可）

**迁移失败处理**：记录日志但不阻断启动；未迁移成功的 AppEntry 保留旧 ExternalId，下次导入时按 project name 重新匹配即可（视为新应用）。

### D9. PortainerClient API 改造

**底层封装保留，业务层不再走 stack。**

`IPortainerClient` 分两层：

**底层 API 封装（全部保留）**：
```csharp
// 已有 —— 保留，供迁移逻辑和未来扩展使用
Task<bool> TestAsync(string configJson);
Task<List<PortainerStackInfo>> ListStacksAsync(string configJson);
Task<List<PortainerPortMapping>> GetStackPortsAsync(string configJson, PortainerStackInfo stack);

// 新增 —— endpoints / projects / project ports
Task<List<PortainerEndpointInfo>> ListEndpointsAsync(string configJson);
Task<List<PortainerProjectInfo>> ListProjectsAsync(string configJson, int endpointId);
Task<List<PortainerPortMapping>> GetProjectPortsAsync(string configJson, int endpointId, string projectName);
```

新增 record：
```csharp
public sealed record PortainerEndpointInfo(int Id, string Name, int Type);
public sealed record PortainerProjectInfo(string Name, int EndpointId, string Status);
```

**业务层切换**：`PortainerAppSource` 从调用 `ListStacksAsync` / `GetStackPortsAsync` 改为调用 `ListProjectsAsync` / `GetProjectPortsAsync`。旧的 stack 方法仅保留给存量迁移使用，迁移完成后可视情况清理。

`ListProjectsAsync` 实现：查容器 → 按 `com.docker.compose.project` 分组 → 每组取一个代表容器获取状态。

### D10. IAppSource 接口调整

```csharp
public interface IAppSource
{
    DataSourceType Type { get; }
    Task<bool> TestConnectionAsync(string configJson);
    Task<List<ExternalCandidate>> ListCandidatesAsync(string configJson, int endpointId);  // 新增 endpointId
    Task<AppDraft> ResolveAsync(string configJson, int endpointId, ExternalCandidate candidate);  // 新增 endpointId
}
```

PortainerAppSource 实现：
- `ListCandidatesAsync`：调用 `ListProjectsAsync(configJson, endpointId)`
- `ResolveAsync`：调用 `GetProjectPortsAsync(configJson, endpointId, projectName)`

### D11. ImportService 幂等匹配

```csharp
var existing = (await _apps.GetAllAppsAsync())
    .FirstOrDefault(a => a.SourceId == source.Id
                      && a.EnvironmentId == environmentId  // 新增
                      && a.ExternalId == draft.ExternalId);
```

## Risks / Trade-offs

- [存量迁移依赖 Portainer API 可达] → 若迁移时 Portainer 不可达，旧 stack Id 无法映射到 project name；处理：记录日志，保留旧 ExternalId，用户下次手动重新导入即可
- [compose project name 不唯一] → 不同 Environment 下可能有同名 project；处理：幂等键包含 EnvironmentId，同 DataSource 下不同 Environment 的同名 project 视为不同应用
- [Environment 自动同步时机] → 每次进入 Sources 页面时自动同步 vs 手动触发；选择：进入 Sources 页面时自动同步（后台静默），同时提供手动「同步」按钮
- [Sources 页面展开区域性能] → 若数据源多、每个数据源下 Environment 多，展开区域可能加载慢；处理：按需展开（点击数据源行时才加载其 Environment 列表）
- [Import 路由变更影响书签] → 旧 `/Admin/Import/{sourceId}` 书签失效；处理：可保留旧路由并重定向到第一个 Environment

## Migration Plan

1. 新增 `PortainerEnvironment` 实体和 `IEnvironmentService`
2. 启动时执行存量迁移（一次性）
3. Sources 页面增加 Environment 展开区域
4. Import 路由改为 `/Admin/Import/{sourceId}/{environmentId}`
5. 应用删除去掉禁用前置条件
6. 验证存量数据迁移正确性

## Open Questions

- 暂无。
