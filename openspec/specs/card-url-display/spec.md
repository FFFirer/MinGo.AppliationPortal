## ADDED Requirements

### Requirement: URL 文本截断

应用卡片中的 URL 文本 SHALL 在超出容器宽度时截断并显示省略号（...）。

#### Scenario: 短 URL 正常显示
- **WHEN** URL 文本长度在容器宽度内
- **THEN** 文本完整显示，不做截断

#### Scenario: 长 URL 截断显示
- **WHEN** URL 文本超出容器宽度
- **THEN** 文本末尾显示 `...`，内容截断

### Requirement: 完整 URL 可查看

用户 SHALL 能通过悬停操作查看被截断的完整 URL。

#### Scenario: 悬停显示 tooltip
- **WHEN** 用户将鼠标悬停在 URL 链接上
- **THEN** 浏览器显示 tooltip，内容为完整的原始 URL
