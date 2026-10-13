# Design: hosts-modal-create-edit

## Context

`/Admin/Hosts` 当前是「列表 + 页底内联新增表单」：

- 列表展示 Name / Addresses / 操作（仅删除）
- 页底 `新增 Host` 表单：`NewName`、`NewAddresses`（多行/逗号分隔），提交到 `OnPostCreateAsync`
- `IHostService` 已有 `CreateAsync` / `UpdateAsync` / `DeleteAsync`，`AccessHost` 含 `Name`、`Addresses`、`IsDefault`、`SortOrder`
- 应用 Create/Edit 页已有「快捷生成」弹窗，但样式与 Host 页独立

目标是把 Host 的新建/编辑统一成模态框，并支持编辑。

## Goals / Non-Goals

**Goals:**

- 新建 Host 使用模态框，移除页底内联表单
- 编辑 Host 与新建共用同一模态框，编辑时回填数据
- 模态框标题与底部操作按钮固定（sticky header/footer，内容区可滚动）
- 后端支持更新 Host 的 Name / Addresses

**Non-Goals:**

- 不改 `AccessHost` 数据模型
- 不改应用 Create/Edit/Import 的 Host 候选逻辑
- 不做 Host 排序拖拽、批量编辑、IsDefault 批量切换 UI
- 不抽通用 Modal 组件库（沿用页面内 CSS/JS 模式）

## Decisions

1. **单模态框 + 模式切换（create / edit）**  
   用一个 modal 节点，通过 `data-mode` / 隐藏 `id` 字段区分新建与编辑。  
   备选：两个独立模态框 — 重复结构多，校验与回填逻辑会分叉，放弃。

2. **表单仍走 Razor Pages POST，不引入 AJAX**  
   新建 → `OnPostCreateAsync`，编辑 → `OnPostUpdateAsync`，成功后 `RedirectToPage`。  
   与现有 `OnPostDeleteAsync` 一致，无 JS 也能工作；模态框只负责收集/回填输入。  
   备选：fetch + JSON 局部刷新 — 收益小，引入状态同步成本，放弃。

3. **地址输入保持多行 textarea**  
   与现逻辑一致（换行/逗号/分号/空格分隔，去重）。编辑时 `string.Join("\n", Addresses)` 回填。

4. **模态框视觉对齐现有 `app.css` 工具类**  
   半透明遮罩 + 居中面板；header 固定标题，footer 固定「取消」「保存」；body 可滚动。  
   备选：原生 `<dialog>` — 样式可控性差、旧浏览器差异，放弃。

5. **隐藏 Id 用字符串提交**  
   编辑表单 `id` 字段提交 ObjectId 字符串，服务端 `IdParser.TryParse`，与删除一致。

## Risks / Trade-offs

- [模态框打开时底层页面仍可滚动] → 遮罩层 `overflow: hidden` 锁定 `body`，关闭后恢复
- [编辑保存失败（校验）] → 重新渲染页并带 `message`，模态框不自动弹出；校验优先在 POST 里拦（Name 必填、至少一个地址）
- [并发编辑覆盖] → 后读前写，LiteDB 单机场景可接受；不在本次加乐观锁
- [与 Create/Edit 页生成器弹窗样式不完全一致] → 本次以 Host 页自洽为主，统一组件库不进范围

## Migration Plan

1. 改 `Hosts.cshtml` / `Hosts.cshtml.cs`，一次发布
2. 回滚 = 还原该页与 PageModel；数据层无迁移

## Open Questions

无。
