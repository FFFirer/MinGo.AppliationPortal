# Design: unify-page-content-width

## Context

当前宽度体系定义在 `AppPortal/Styles/app.css`：

- `.page-shell`：`max-w-7xl`（80rem / 1280px）+ 响应式 padding
- `.page-shell-narrow`：`max-w-3xl`（48rem / 768px）+ 同一套 padding
- 小屏统一收紧：`px-4 py-5 sm:px-6 sm:py-8 lg:px-8`

页头（`_Layout.cshtml`）与页脚固定使用 `page-shell`，因此 chrome 永远 1280px。正文却分两档，另有 `Admin/Delete.cshtml` 裸写 `max-w-lg mx-auto px-4 sm:px-6 py-8`（512px），形成第三档。

页面映射现状：

| 页面 | 壳层 | 内容形态 |
|------|------|----------|
| Index（首页） | page-shell | 卡片网格 |
| Detail | page-shell-narrow | 信息卡 |
| Admin/Index | page-shell | 数据表 |
| Admin/Create、Edit | page-shell-narrow | 表单 |
| Admin/Delete | 裸 max-w-lg | 确认卡 |
| Admin/Hosts、Sources | page-shell-narrow | 数据表（被压窄） |
| Admin/Import | page-shell-narrow | 候选表单 |

## Goals / Non-Goals

**Goals:**

- 全站正文单一宽度档，与页头/页脚 `page-shell` 对齐
- 消除页级 `max-w-*` 逃逸（Delete）
- Hosts / Sources 等表格页获得与应用列表同等列空间
- 实现面改动收敛在 shell 类名替换 + 一条 CSS 清理，不引入新布局抽象

**Non-Goals:**

- 不改卡片、表格、表单控件内部尺寸语义
- 不为表单增加「壳层内再收窄」的第二层约束（那是另一套方向 C）
- 不改首页卡片列数、不改断点
- 不改页头/页脚 DOM
- 不做视觉重设计（颜色、圆角、间距节奏保持）

## Decisions

### D1. 单一宽度取 `max-w-7xl`（1280px），而不是中间档

**选择**：正文与 chrome 同用 `page-shell` = 1280px。

**理由**：

- 页头/页脚已是 1280px，直接对齐零冲突
- 管理列表、首页网格在 1280px 下已验证可读
- Hosts/Sources 表格受益最大，正好是现状痛点
- 若改成 1024px 中间档，页头也要跟着改，改动面更大，且列表页会白白变窄

**备选**：

- `max-w-5xl`（1024px）：表单更易读，但 chrome 需同步收窄，列表变紧 —— 放弃
- 保持两档按内容归位：仍存在跨页宽度跳变，不符合「统一」目标 —— 放弃

### D2. 废弃 `page-shell-narrow`，不保留空壳类

**选择**：从 `Styles/app.css` 删除 `.page-shell-narrow` 定义，所有引用改为 `page-shell`。小屏 padding 规则里对 narrow 的拼合选择器同步删掉。

**理由**：

- 一旦无页面引用，保留定义只会制造第三种「有人可能再用」的歧义
- 本仓库样式源在 `Styles/app.css`，`wwwroot/css/app.css` 为构建产物，删干净可 grep 验证

**备选**：保留类并 `@deprecated` 注释 —— 无调用方时属于死代码，按仓库惯例直接删除。

### D3. 表单不在壳层内二次收窄

**选择**：新增/编辑表单在 1280px 壳层内铺满卡片，不给 `.input` / 表单卡加 `max-w-3xl`。

**理由**：

- 方向 A 的定义就是「正文宽度统一」；内部再收窄等于又造出第二套宽度
- 编辑页模块块含多行 URL，变宽反而减少换行
- 若日后觉得输入框过长，可在表单字段层做可读性优化，不回退壳层

### D4. Delete 页并入 `page-shell`，确认卡保持居中窄卡

**选择**：外层容器改 `page-shell`；内部确认卡片/警告块仍可居中（例如保留 `max-w-lg mx-auto` 作为**卡片自身**宽度，而非页面壳层）。

**理由**：

- 逃兵的是「页面壳层」裸写 padding + max-width；确认卡本身窄是内容设计
- 与其它页一致：壳层统一，内容块自己决定要不要收窄

## Risks / Trade-offs

- [表单行过长] → 变宽是方向 A 的已知代价；模块块/URL 行受益，名称/描述类字段可接受。若反馈差，在字段层加 `max-w-*`，不回退壳层
- [详情页/删除页两侧空白变多] → 与 chrome 对齐后的正常结果；内容卡本身未拉伸到无意义长度时可在卡内用 `max-w-lg mx-auto`（Delete 已按此处理）
- [wwwroot 构建产物不同步] → 改完 `Styles/app.css` 后重建 CSS（项目 Tailwind v4 / parcel 流程），并 grep 确认 `page-shell-narrow` 无残留
- [历史 archive 规格提到布局] → 不改已归档 spec；新能力 `page-content-width` 独立成文

## Migration Plan

纯前端 class 替换，无数据/路由/模型变更。回滚 = 还原 class 名与 CSS。

1. 改 `Styles/app.css`：删 narrow、保留 shell、清理拼合选择器
2. 替换各 cshtml 引用
3. 重建 `wwwroot/css/app.css`
4. 走查：首页 / 列表 / 详情 / 新增 / 编辑 / 删除 / Hosts / Sources / Import，确认无横向滚动、无窄栏残留

## Open Questions

（无）
