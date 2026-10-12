# Proposal: url-addresses-generator

## Why

地址配置被拆成 Host/协议/端口/路径多个控件后，录入一条真实 URL 仍繁琐；相近地址（同 path 多 IP、同 host 多端口）只能整行重填。需要「直接写 URL」为主路径，并用弹窗生成器按配置**一次批量生成多条**。

## What Changes

- **地址改为纯 URL 字符串**：`AppModule.Addresses: List<string>`，不再结构化存 Host/端口/路径
- **复制行**：仅复制 URL 文本，不携带 Host 关联
- **快捷生成弹窗**：模块级上下文；协议必选；地址/端口/路径支持候选、约束、自定义
- **批量生成**：多选 IP（或 Host 下多 IP）× 协议 × 端口 × 路径 组合出多条 URL，一次追加
- Host 目录保留，仅作生成器候选 IP 池

## Capabilities

### Modified Capabilities

- `app-portal-storage`: 模块地址存储改为 `List<string>` URL 集合
- `app-portal-admin`: 表单改为 URL 行 + 复制 + 生成器弹窗（可多条）

## Impact

- `AppModule` / `AppEditModel` / 详情展示 / 导入映射
- Create/Edit：URL 列表 + 弹窗生成器 JS
- 无新 NuGet；Host 服务仍用于候选地址
