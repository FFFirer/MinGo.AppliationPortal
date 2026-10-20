# Tasks: datasource-host-binding

## 1. 数据模型与存储

- [x] 1.1 `AppPortal/Models/DataSource.cs` 增加 `HostIds: List<ObjectId>`（缺省 `new()`；读取旧文档为 null 时 `??= new()`）
- [x] 1.2 确认 `app-portal-storage` 增量规格与模型一致（HostIds 语义：空 = 未绑定）
- [x] 1.3 存量 DB 无字段读写回归：未绑定数据源导入行为与现状一致

## 2. 数据源创建 / 编辑绑定 UI

- [x] 2.1 `Sources.cshtml(.cs)` 创建表单增加 Host 多选（可选，默认不选 = 未绑定）
- [x] 2.2 增加数据源编辑入口（模态或编辑 handler），可维护 HostIds；保存走 `UpdateAsync`
- [x] 2.3 编辑入口不与 `datasource-name-unique-key` 的 Name 不可改约束冲突（本 change 不改 Name）
- [x] 2.4 走通创建绑定 → 列表可见；编辑改绑定 → 再次编辑回填

## 3. 导入候选池与多选

- [x] 3.1 `Import.cshtml.cs`：`GenHostId` 改为 `GenHostIds`（多选）；默认全选候选池
- [x] 3.2 候选池：`Source.HostIds` 非空且有有效 Host → 仅绑定集合；否则全量；悬空 Id 过滤
- [x] 3.3 绑定集合过滤后为空 → 按未绑处理展示全量并提示「绑定已失效」
- [x] 3.4 `Import.cshtml` 主机字段改多选控件；IP 预览展示各选中 Host 全部 IP（跨 Host 去重）

## 4. 地址展开

- [x] 4.1 `RewriteUrlsFromPortLabels` 按所有选中 Host 的 IP 并集展开端口，去重后生成 URL，默认全选
- [x] 4.2 保留手工取消勾选 / 改 URL 后「确认导入」只写入选中项
- [x] 4.3 多 Host × 多 IP 下列表可用（预览汇总已选 IP 数；无自定义 IP 输入）

## 5. 回归与验收

- [x] 5.1 未绑定数据源导入：全量 Host、可多选、默认全选、展开正确
- [x] 5.2 已绑定数据源导入：仅绑定 Host、默认全选、展开正确
- [x] 5.3 绑定 Host 删除后导入：悬空过滤 / 失效提示 / 回落全量
- [x] 5.4 再导入更新路径（SourceId+ExternalId）不被本 change 破坏
