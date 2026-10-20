# Design: datasource-name-unique-key

## Context

数据源身份现状：

- `DataSource.Id` 为 LiteDB `ObjectId`，是路由（`/Admin/Import/{sourceId}`）与 `AppEntry.SourceId` 外键的唯一依据
- `DataSource.Name` 为自由文本展示字段，无唯一约束、无字符集限制（中文/空格均可）
- `PortainerClient.CreateClient` 对所有数据源使用同一个 HttpClient 名 `"portainer"`，每次调用覆盖 `BaseAddress` 与认证头；多数据源并发请求时共享 handler 池上的实例级配置会互相污染
- `datasources` 集合未对 `Name` 建索引（对比 `apps` / `hosts` 已有 `EnsureIndex(x => x.Name)`）

相关约束（已与需求方对齐）：

- `Name` 即可视化唯一键，不引入独立中文 Display
- `Name` 创建后不可修改
- 字符集仅 `[A-Za-z0-9_-]`
- HttpClient 名采用**类型前缀**拼接
- `ObjectId` 保留，不替换主键

## Goals / Non-Goals

**Goals:**

- `Name` 全局唯一、可校验、创建后冻结，三位一体（唯一键 = 展示名 = client 键基础）
- Portainer client 按数据源隔离 HttpClient 线路，消除多源并发竞态
- 创建入口给出明确校验反馈（非法字符 / 重名 / 超长）
- `datasources.Name` 建唯一索引，服务层兜底校验

**Non-Goals:**

- 不替换 `ObjectId` 主键，不改 `AppEntry.SourceId` 类型
- 不改 `/Admin/Import/{sourceId}` 路由与 `IdParser`
- 不引入 Display/别名/改名/别名表
- 不做存量数据批量迁移（空库或仅校验新增）
- 不覆盖 ComposeFile/Manual 源的业务导入逻辑（仅约定其 client/键拼接命名空间）

## Decisions

### D1. Name 三位一体，ObjectId 仅作 `_id` 与外键

**选择**：`Name` 是业务唯一键 + 展示名 + HttpClient 名的拼接成分；`ObjectId` 继续作 LiteDB `_id`、`AppEntry.SourceId`、路由参数。

**理由**：改名禁止后，Name 天然稳定，适合对外可见与配置拼接；保留 ObjectId 让导入路由、再导入匹配、外键几乎零改动。

**备选**：Name 直接作字符串 `_id`——更纯粹，但 `AppEntry.SourceId`、`IdParser`、路由全量迁移，收益不成比例。

### D2. HttpClient 名格式：`{typePrefix}:{Name}`

**选择**：`CreateClient` 使用 `portainer:{Name}`（Portainer）；未来其他 Type 使用各自前缀（如 `composefile:{Name}`）。

**理由**：类型前缀隔离多 Type 命名空间；冒号分隔可读且在 HttpClient 名中安全；Name 字符集已保证无特殊字符。

**备选**：
- 裸 Name（`prod-01`）——跨 Type 同名冲突
- 连接符 `portainer-prod-01`——与 Name 内 `-` 混淆，解析不清晰

### D3. Name 校验规则

| 规则 | 取值 |
|------|------|
| 字符集 | `^[A-Za-z0-9_-]+$` |
| 首字符 | 不得为 `-` 或 `_` |
| 长度 | 1–64 |
| 唯一范围 | **全局唯一**（跨 Type） |
| 大小写 | **敏感**（`Prod` 与 `prod` 可共存） |
| 改名 | 禁止（无 Update-Name 入口） |
| 删除后 | 同名可复用 |

**理由**：全局唯一保证展示无歧义、client 键无歧义；大小写敏感与 LiteDB 默认字符串比较一致，避免隐式折叠。

### D4. 校验位置：服务层 + 创建表单

- `DataSourceService.CreateAsync`：字符集/长度/唯一性（重名抛错或返回失败）
- `SourcesModel.OnPostCreateAsync`：表单级提示（名称必填、字符非法、已存在）
- 无编辑 Name 的 UI/接口（现状 Sources 页本就无编辑入口，保持）

### D5. 唯一索引

```csharp
_sources.EnsureIndex(x => x.Name, unique: true);
```

在 `DataSourceService` 构造时执行。服务层仍做显式校验，保证错误信息可读（索引冲突异常不直接抛给 UI）。

### D6. PortainerClient 改造

调用链需要把「类型 + Name」传到 client 构建处：

- `IPortainerClient` 方法签名从 `(string configJson)` 调整为携带 source 标识（如 `TestAsync(DataSource source)` 或 `(string clientKey, string configJson)`）
- `CreateClient` 内部：`_httpClientFactory.CreateClient($"{typePrefix}:{name}")`，再设置该实例的 BaseAddress 与认证头
- 每个数据源独立 handler 名，互不覆盖

`PortainerConfig` 仍从 `source.Config` 反序列化，不变。

## Risks / Trade-offs

- [存量非法 Name 撞唯一索引] → 新建校验只管新增；若库中已有重名，`EnsureIndex(unique)` 可能失败——实现时先探测并给出可诊断错误，或启动时跳过脏数据提示
- [Name 敏感带来运维不便] → 刻意取舍：Name 是机器键兼展示名，创建时即锁定；误创建只能删后重建
- [HttpClient 名随 Name 永久占用] → 删除数据源后 Name 可复用；handler 池由 `IHttpClientFactory` 自然回收过期 handler，无需手工清理
- [签名变更牵动调用侧] → `PortainerAppSource` / `DataSourceService.TestConnectionAsync` 需同步传 Name/Type；编译期可发现全部调用点
- [大小写敏感] → `Prod`/`prod` 两个 client 名与两条数据源并存；若未来要折叠，需迁移而非静默改比较规则

## Migration Plan

1. 代码合入后首次启动：`EnsureIndex(x => x.Name, unique: true)`
2. 空库/合规数据：无操作
3. 存在非法或重名 Name：创建新源仍受校验；索引创建失败时记录日志并提示人工清理（不自动改名）
4. 回滚：恢复旧代码即可；唯一索引可留可删，旧逻辑不依赖它

## Open Questions

- 暂无。默认细则已对齐：全局唯一、禁 `-_` 开头、1–64、大小写敏感、删除后可复用、不做存量迁移。
