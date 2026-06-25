## ADDED Requirements

### Requirement: 卡片名称文本截断

应用卡片标题 SHALL 在超出容器宽度时截断并显示省略号（...）。

#### Scenario: 短名称正常显示
- **WHEN** 名称文本长度在容器宽度内
- **THEN** 文本完整显示，不做截断

#### Scenario: 长名称截断显示
- **WHEN** 名称文本超出容器宽度
- **THEN** 文本末尾显示 `...`，内容截断

### Requirement: 完整名称可查看

用户 SHALL 能通过悬停操作查看被截断的完整名称。

#### Scenario: 悬停显示 tooltip
- **WHEN** 用户将鼠标悬停在卡片标题上
- **THEN** 浏览器显示 tooltip，内容为完整名称
