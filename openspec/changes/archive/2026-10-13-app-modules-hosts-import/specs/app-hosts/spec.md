## ADDED Requirements

### Requirement: 全局 Host 目录
系统 SHALL 维护全局 Host 目录，每条 Host 包含 Name、Address、SchemeDefault、IsDefault、SortOrder。Host 可被多个应用引用。

#### Scenario: Host 字段
- **WHEN** 创建或编辑 Host
- **THEN** 必填 Name 与 Address
- **THEN** SchemeDefault 默认 http，IsDefault 与 SortOrder 可选

#### Scenario: 全局 IsDefault
- **WHEN** 存在 IsDefault=true 的 Host
- **THEN** 应用未指定 Preferred Host 时，详情页默认使用该 Host

### Requirement: 应用引用 Host
`AppEntry` SHALL 支持配置 `HostRefs`（HostId 列表），声明该应用可在哪些 Host 下访问。至少包含一个 Host 引用才能解析拼装 URL。

#### Scenario: 应用绑定多个 Host
- **WHEN** 管理员为应用配置「办公网」「公网」两个 HostRef
- **THEN** 详情页 Host 切换器展示这两项

#### Scenario: Preferred Host
- **WHEN** 应用的某个 HostRef 标记为 Preferred
- **THEN** 详情页默认选中该 Host（优先于全局 IsDefault）

### Requirement: 入口级 Host 绑定与 URL 拼装
`AppEndpoint` SHALL 支持可选 `HostId`（空则用应用默认 Host）、Port、ContainerPort、Path、SchemeOverride、UrlOverride、Kind(Http|Tcp|Udp)。

#### Scenario: 默认拼装 Http URL
- **WHEN** 入口 Kind=Http，未设 UrlOverride，Port=8080，Path=/admin，当前 Host Address=10.0.0.5，SchemeDefault=http
- **THEN** 解析 URL 为 `http://10.0.0.5:8080/admin`

#### Scenario: 端口省略
- **WHEN** 拼装 Http URL 且 Port 为空
- **THEN** https 使用 443 并省略端口段，http 使用 80 并省略端口段

#### Scenario: UrlOverride 覆盖拼装
- **WHEN** 入口设置了 UrlOverride
- **THEN** 解析结果等于 UrlOverride，忽略 Host/Port/Path

#### Scenario: Tcp 入口展示
- **WHEN** 入口 Kind=Tcp 且 Port=3306
- **THEN** 详情展示 `Host.Address:3306` 文本，不生成链接

#### Scenario: 入口绑定指定 Host
- **WHEN** 入口 HostId 指向「公网」而当前切换为「办公网」
- **THEN** 该入口在「办公网」下视为不可达（除非 UrlOverride）
