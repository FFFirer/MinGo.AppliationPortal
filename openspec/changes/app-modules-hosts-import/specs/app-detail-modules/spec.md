## ADDED Requirements

### Requirement: 应用详情页展示模块入口
系统 SHALL 提供应用详情页 `/Detail/{id}`，按 `AppModule` 分区展示各模块下的访问入口。卡片点击后导航到详情页，而非直接打开外链。

#### Scenario: 打开应用详情
- **WHEN** 用户在首页点击某个应用卡片
- **THEN** 导航到该应用详情页
- **THEN** 展示应用名称、描述、图标（若有）

#### Scenario: 按模块分组展示入口
- **WHEN** 应用包含多个模块且每个模块有若干入口
- **THEN** 详情页按模块 Order 升序分节展示
- **THEN** 每节显示模块名称，节内按入口顺序列出 DisplayName 与解析后的地址

#### Scenario: 禁用模块不展示
- **WHEN** 某模块 `IsEnabled = false`
- **THEN** 详情页不展示该模块及其入口

#### Scenario: Http 入口新标签打开
- **WHEN** 用户点击 Kind=Http 的入口且当前 Host 下有可达 URL
- **THEN** 浏览器在新标签页打开该 URL（`target="_blank"` + `rel="noopener noreferrer"`）

#### Scenario: 非 Http 入口不可点击
- **WHEN** 入口 Kind 为 Tcp 或 Udp
- **THEN** 仅展示 `address:port` 文本，不渲染为可跳转链接

### Requirement: 多 Host 切换展示
详情页 SHALL 提供 Host 切换器，列出该应用 `HostRefs` 中的 Host；切换后所有入口按所选 Host 解析地址。

#### Scenario: 默认 Host
- **WHEN** 打开详情页
- **THEN** 默认选中应用 Preferred Host（或全局 IsDefault Host，或列表第一个）

#### Scenario: 切换 Host 后入口地址变化
- **WHEN** 用户将 Host 从「办公网」切换到「公网」
- **THEN** 各 Http 入口展示的 URL 按公网 Host 的 Address 与入口 Port/Path 重新拼装

#### Scenario: 记住上次 Host 选择
- **WHEN** 用户切换 Host 后再次打开任意应用详情
- **THEN** 默认 Host 使用浏览器 localStorage 中记住的选择（若该应用包含此 Host）

### Requirement: 入口不可达展示
当前 Host 下无法解析的入口 SHALL 灰显提示，不提供可点击链接；模块下全部入口不可达时 SHALL 折叠该模块。

#### Scenario: 单入口无 Host 映射
- **WHEN** 当前 Host 下某入口无有效地址（未绑定且无法用默认拼装）
- **THEN** 该入口行灰显并显示「当前网络不可达」
- **THEN** 不渲染可点击链接

#### Scenario: 模块全部不可达
- **WHEN** 当前 Host 下某模块所有入口均不可达
- **THEN** 该模块默认折叠
- **THEN** 模块标题旁标注不可达入口数（如「3 项不可达」）

#### Scenario: UrlOverride 优先
- **WHEN** 入口设置了 UrlOverride
- **THEN** 无论当前 Host 为何，点击均打开 UrlOverride 地址
