# Tasks: datasource-name-unique-key

## 1. 数据源 Name 约束与唯一索引

- [ ] 1.1 在 `DataSourceService` 为 `datasources.Name` 建唯一索引（`EnsureIndex(x => x.Name, unique: true)`）
- [ ] 1.2 实现 Name 校验（`^[A-Za-z0-9_-]+$`、禁 `-_` 开头、长度 1–64）与全局重名检查，`CreateAsync` 拒绝非法/重名并返回可展示错误
- [ ] 1.3 确认数据源无改名入口（无 Update-Name）；删除后同名可复用（默认可复用，补一条回归确认）

## 2. Portainer client 唯一键拼接

- [ ] 2.1 调整 `IPortainerClient` / `PortainerClient`，构建 HttpClient 时按 `{typePrefix}:{Name}`（Portainer 为 `portainer:{Name}`）创建独立 client
- [ ] 2.2 串联调用侧（`PortainerAppSource`、`DataSourceService.TestConnectionAsync`、`ImportService`），把 Type+Name 传入 client 构建
- [ ] 2.3 验证多数据源并发请求时 BaseAddress/API Key 互不覆盖

## 3. 管理端创建校验与提示

- [ ] 3.1 `SourcesModel.OnPostCreateAsync` 增加 Name 字符集/长度/唯一性校验，失败时提示具体原因（非法字符 / 已存在 / 超长）
- [ ] 3.2 `/Admin/Sources` 创建表单提示文案对齐（占位符/帮助文案标明仅 `[A-Za-z0-9_-]`、创建后不可改）

## 4. 验证

- [ ] 4.1 走通合法创建（`prod-01`）→ 测试连接 → 导入页可用
- [ ] 4.2 走通非法 Name（中文、`.`、`/`、`-` 开头、超长）被拒且有提示
- [ ] 4.3 走通重名拒绝、删除后同名重建成功
- [ ] 4.4 多 Portainer 数据源并发下 client 配置隔离（日志或行为确认 `portainer:{Name}`）
