## 1. 数据模型与存储

- [x] 1.1 新增 Host、AppModule、AppEndpoint、HostRef、DataSource 模型，调整 AppEntry
- [x] 1.2 扩展 IAppService/AppService，新增 IHostService/HostService、IDataSourceService/DataSourceService
- [x] 1.3 实现旧 Urls → 默认模块的惰性迁移
- [x] 1.4 注册服务到 Program.cs

## 2. URL 解析与 Host 服务

- [x] 2.1 实现 UrlResolver（拼装、UrlOverride、端口省略、Tcp 文本）
- [x] 2.2 实现入口可达性判断与不可达标记

## 3. 首页与详情页

- [x] 3.1 首页卡片只展示身份信息，整卡链到详情
- [x] 3.2 新增 /Detail/{id}：模块分区、入口列表
- [x] 3.3 详情页 Host 切换器 + localStorage 记忆
- [x] 3.4 不可达灰显与全空模块折叠

## 4. 管理后台表单

- [x] 4.1 Create/Edit 改为模块 + 入口动态表单，含 HostRefs
- [x] 4.2 管理列表增加来源列

## 5. 数据源与 Portainer 导入

- [x] 5.1 DataSource 实体服务与 /Admin/Sources 列表/创建/测试连接
- [x] 5.2 IAppSource + AppDraft 契约与 PortainerClient（stacks/containers）
- [x] 5.3 /Admin/Import：候选列表、AppDraft 预览、勾选导入
- [x] 5.4 SourceId+ExternalId 幂等更新与 SyncHint 漂移提示

## 6. 验证

- [x] 6.1 dotnet build 通过
- [x] 6.2 核心路径手测（卡片→详情、模块、Host 切换、导入预览数据结构）
