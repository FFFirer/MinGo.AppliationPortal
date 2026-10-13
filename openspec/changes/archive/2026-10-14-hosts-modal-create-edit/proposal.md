# Proposal: hosts-modal-create-edit

## Why

Host 目录页目前只有页底内联「新增 Host」表单，列表没有编辑入口，改名或改地址只能删除重建，且新建流程与应用页已有的快捷生成弹窗交互不一致。需要支持编辑，并把新建/编辑统一为模态框操作，固定标题与操作按钮，减少打断。

## What Changes

- Host 列表每行增加「编辑」操作
- 「新增」从页底内联表单改为模态框
- 「编辑」与「新增」共用同一模态框，编辑时回填 Name / Addresses
- 模态框标题固定（「新增 Host」/「编辑 Host」），底部操作按钮固定（取消 / 保存）
- 后端增加 Host 更新处理，复用 `IHostService.UpdateAsync`
- 删除页底内联新增表单

## Capabilities

### Modified Capabilities

- `app-hosts`: 创建 Host 改为模态框；新增编辑 Host 需求；交互统一固定标题与操作按钮

## Impact

- `AppPortal/Pages/Admin/Hosts.cshtml`：列表操作列、模态框 UI
- `AppPortal/Pages/Admin/Hosts.cshtml.cs`：新增 `OnPostUpdateAsync`，调整创建入参
- `IHostService` / `HostService`：`UpdateAsync` 已存在，无需改接口
- 无新 NuGet；不影响应用 Create/Edit/Import 的 Host 候选逻辑
