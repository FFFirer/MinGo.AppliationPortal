# Tasks: hosts-modal-create-edit

## 1. 后端

- [x] 1.1 `HostsModel` 增加编辑用绑定字段（`EditId` / `EditName` / `EditAddresses`）与 `OnPostUpdateAsync`，校验规则与创建一致
- [x] 1.2 保留 `OnPostCreateAsync`，失败时通过 `message` 回传；成功后 `RedirectToPage`

## 2. 前端模态框

- [x] 2.1 `Hosts.cshtml` 新增共用模态框结构：固定标题区 + 可滚动表单区 + 固定底部「取消」「保存」
- [x] 2.2 移除页底「新增 Host」内联表单，改为「新增 Host」按钮打开模态框（create 模式）
- [x] 2.3 列表操作列增加「编辑」，点击后打开同一模态框（edit 模式）并回填 Name / Addresses
- [x] 2.4 补齐打开/关闭脚本：标题切换「新增 Host」/「编辑 Host」、遮罩点击与取消关闭、body 滚动锁定

## 3. 验证

- [x] 3.1 走通新建：空 Name / 空地址校验提示，成功后列表出现新 Host
- [x] 3.2 走通编辑：回填正确，保存后 Name/Addresses 更新，多地址（多行）往返正确
- [x] 3.3 检查模态框标题与操作按钮固定，内容滚动时 header/footer 不动；取消/遮罩关闭不提交
