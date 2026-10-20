# Design: datasource-host-binding

## Context

导入页 `Import.cshtml(.cs)` 当前始终 `HostService.GetAllAsync()` 全量 Host，`GenHostId` 单选，`RewriteUrlsFromPortLabels` 将所选 Host 全部 IP 展开为地址。`DataSource` 无 Host 字段；`Sources` 页只有创建表单，没有编辑。规格 `app-hosts` 明确写了「导入下拉单选 Host」。

需求：数据源绑定 Host 作为导入候选池（绑定则硬过滤，未绑则全量）；导入主机多选，默认全选。

与 `datasource-name-unique-key`（in-progress）同改 `DataSource` 实体但互不依赖，独立 change 落地，避免 scope 膨胀。

## Goals / Non-Goals

**Goals:**

- `DataSource.HostIds` 持久化绑定，创建 + 编辑可维护
- 导入 Host 候选池：绑定 → 仅绑定集合；未绑 → 全局目录
- 主机多选 + 默认全选；地址按「选中 Host × 其全部 IP」展开
- 存量数据零迁移（缺省 `HostIds` 为空 = 未绑）

**Non-Goals:**

- 自定义 IP 输入 / Host 以外的地址源
- 应用级 HostRefs、入口级 Host 绑定
- 删除 Host 时的级联清理（导入时过滤悬空 Id 即可）
- 独立「绑定 Host」页面（并入 Sources 创建/编辑）
- 与 Name 唯一键变更合并

## Decisions

### D1. 绑定存 `DataSource.HostIds: List<ObjectId>`

- **选择**：一等字段 `List<ObjectId>`，与 `AppEntry.SourceId` 同风格软引用
- **备选**：塞进 `Config` JSON → 可，但查询/编辑都要解析 JSON，且 Config 语义是 Type 专有连接配置
- **理由**：Host 绑定是跨 Type 的导入行为，不是 Portainer 连接参数；一等字段便于 LiteDB 读写与表单绑定

### D2. 绑定后硬过滤候选池

- **选择**：`HostIds` 非空 → 候选仅含绑定且仍存在的 Host；空 → 全量
- **备选**：软优先（绑定排前，仍可选其它）→ 「优先」语义含糊，导入时易选错
- **理由**：与需求「以数据源绑定的 host 为准，若无绑定则按所有 host」一致；过滤即约束

### D3. 主机多选 `GenHostIds: List<string>`

- **选择**：checkbox 多选（或 multi-select），默认勾选候选池全部；`RewriteUrlsFromPortLabels` 收集所有选中 Host 的 IP 并集再展开
- **备选**：保持单选 → 无法「一次按多 Host 生成」
- **IP 去重**：跨 Host IP 并集后 `Distinct`，避免同 IP 重复 URL
- **展开密度**：端口 × IP；多 Host 时可能条数较多，沿用「默认全选可取消」交互

### D4. 编辑入口：Sources 表列表 + 创建表单

- **选择**：创建表单增加 Host 多选（可选）；列表行增加「编辑」打开模态框，可改绑定（及以后可扩展字段）；`Name` 若受 unique-key change 约束不可改则编辑表单不含 Name
- **备选**：仅创建时绑 → 已有数据源无法补绑
- **协调**：若 `datasource-name-unique-key` 先落地，编辑模态不提供改 Name；Host 绑定与其正交

### D5. 悬空 HostIds

- **选择**：导入解析候选时 `HostIds` 与现存 Host 求交；交为空视同未绑（回落全量）或提示「绑定的 Host 已失效」
- **采用**：交为空 → 按未绑处理展示全量，并在导入页提示绑定失效；不级联删绑定

## Risks / Trade-offs

- [多 Host × 多 IP 地址列表膨胀] → 默认全选可取消；预览区汇总已选 IP 数
- [与 name-unique-key 并发改 `DataSource`] → 字段正交，冲突面小；后落地者 rebase 解决模型文件
- [编辑模态范围不清] → 本 change 只保证 HostIds 可编辑；Name/Type/连接串是否可改跟 unique-key 决策对齐
- [Host 删除后悬空 Id] → 过滤 + 提示，不自动改数据

## Migration Plan

1. 模型加 `HostIds`（LiteDB 缺字段反序列化为 null → 代码侧 `??= new()`）
2. Sources 创建/编辑接线
3. Import 多选 + 候选池过滤 + 展开逻辑
4. 回滚：字段可留，UI 回退单选即回到旧行为；无破坏性数据变更

## Open Questions

- 编辑模态是否同时允许改 BaseUrl/ApiKey（倾向：本 change 可先只做 Host 绑定编辑，连接配置另说）
- 候选池交为空时「回落全量」是否要加开关（默认回落 + 提示，已写入 D5）
