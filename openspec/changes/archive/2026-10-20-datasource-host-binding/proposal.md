# Proposal: datasource-host-binding

## Why

外部应用导入的「批量改写地址」目前永远列出全局 Host 目录且只能单选，与数据源无关。多环境（同栈不同机房/网段）下每次导入都要在无关 Host 里挑，容易选错；也无法一次按多个 Host 展开地址。需要数据源绑定 Host 作为导入候选池，并在导入时多选。

## What Changes

- `DataSource` 增加 `HostIds`（绑定的 `AccessHost.Id` 列表），支持在数据源**创建与编辑**时维护
- 导入预览的 Host 候选池：
  - 数据源已绑定 Host → **仅展示绑定的 Host**（硬过滤，默认多选且全选）
  - 未绑定任何 Host → 展示全部 Host（多选，默认全选）
- 「批量改写地址」主机字段由**单选**改为**多选**；每个选中 Host 的全部 IP 参与生成
- 同一服务端口按「选中 Host × 该 Host 全部 IP」展开为多条 URL，默认全选，仍可逐条取消/改写
- 不自定义 IP 输入，候选仍只来自 Host 目录
- 不改 `AppEntry` / 地址落库结构（仍是 URL 字符串列表）
- 与 `datasource-name-unique-key` 互不合并，独立落地

## Capabilities

### New Capabilities

（无）

### Modified Capabilities

- `app-import-sources`: 数据源可绑定 Host；导入 Host 候选池优先绑定集合否则全量；主机多选与地址笛卡尔展开
- `app-hosts`: 「导入下拉单选」改为「导入多选 + 数据源绑定优先」
- `app-portal-storage`: `DataSource` 增加 `HostIds` 字段

## Impact

- `AppPortal/Models/DataSource.cs` — 新增 `HostIds: List<ObjectId>`
- `AppPortal/Pages/Admin/Sources.cshtml(.cs)` — 创建表单可选绑定 Host；列表增加编辑/绑定入口（新建编辑能力）
- `AppPortal/Pages/Admin/Import.cshtml(.cs)` — `GenHostId` → 多选 `GenHostIds`；候选池过滤；`RewriteUrlsFromPortLabels` 按多 Host 展开
- `openspec/specs/app-import-sources/spec.md`、`app-hosts/spec.md`、`app-portal-storage/spec.md` — 同步需求与场景
- 存量数据：`HostIds` 缺省为空 = 未绑定，行为与现状一致（展示全部 Host）
- 已删除 Host 的悬空 `HostIds` 在导入时忽略并过滤出有效候选
