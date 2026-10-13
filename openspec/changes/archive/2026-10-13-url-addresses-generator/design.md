## Context

应用模块下已有多行地址配置。当前为结构化字段（Host/协议/端口/路径），录入完整 URL 与「多 IP 同配置」成本高。

## Goals / Non-Goals

**Goals:**
- 模块地址 = URL 字符串集合
- 弹窗生成器：分段候选/约束/自定义，支持组合生成多条
- 复制行只复制字符串

**Non-Goals:**
- 在应用文档内维护 Host 关联
- 结构化回写/按段编辑

## Decisions

### D1. 存储 `List<string>`
URL 即真相；schema 必填（http/https）。生成/复制/手输都是字符串进表。

### D2. 生成器弹窗（模块上下文）
字段：
- 协议：http|https（必选）
- 地址：多选 IP（来自 Host 目录）+ 自定义附加
- 端口：多选常用 + 自定义（可空）
- 路径：单值（可空）+ 常用建议

**批量规则**：`addresses × ports(若多选) × 1 path` 笛卡尔（协议单选），或 `addresses × 1 port × paths(若多选)`。避免无意义爆炸：端口与路径不可同时多选（约束）。

### D3. 复制
`copy(url)` → 新行同字符串，可改；不带 HostId。

### D4. 校验
必须匹配 `^https?://`；其余宽松（允许 IP、带端口、path）。

## Risks

- [生成条数过多] → 预览列表 + 上限提示（如 50）
- [字符串无法回显 Host] → 可接受；生成时已选 Host 仅作候选

## Open Questions

无。
