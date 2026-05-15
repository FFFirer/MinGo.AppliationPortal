## Purpose

定义 MinGo AppliationPortal 的 Docker Compose 部署配置规范。通过编排文件定义 app-portal 服务、端口映射、环境变量和 LiteDB 数据卷持久化。

## Requirements

### Requirement: Docker Compose 编排文件
项目根目录 SHALL 包含 `docker-compose.yml`，定义 `app-portal` 服务的容器化部署配置。

#### Scenario: 服务定义
- **WHEN** 用户查看 `docker-compose.yml`
- **THEN** 定义名为 `app-portal` 的服务
- **THEN** 服务使用 `build` 上下文从当前目录构建镜像
- **THEN** 服务使用 `image` 指向私有 Registry 的镜像

### Requirement: 端口映射
服务 SHALL 将主机端口映射到容器内 8080 端口。

#### Scenario: 端口暴露
- **WHEN** 执行 `docker compose up`
- **THEN** 主机 `8080` 端口映射到容器 `8080` 端口
- **THEN** 通过 `http://localhost:8080` 可访问应用

### Requirement: 环境变量配置
服务 SHALL 通过 `environment` 配置以下环境变量：
- `ASPNETCORE_ENVIRONMENT=Production`
- `LiteDb__Path=/data/AppPortal.db`

#### Scenario: 生产环境配置
- **WHEN** 容器启动
- **THEN** `ASPNETCORE_ENVIRONMENT` 为 `Production`
- **THEN** 生产环境配置（错误页面、HSTS 等）生效

#### Scenario: 数据库路径配置
- **WHEN** 容器启动
- **THEN** LiteDB 数据库文件存储在 `/data/AppPortal.db`

### Requirement: LiteDB 数据卷持久化
服务 SHALL 使用 Docker 命名 volume 持久化 LiteDB 数据文件。

#### Scenario: Volume 定义
- **WHEN** 定义 `volumes` 配置
- **THEN** 命名 volume `app-portal-data` 挂载到容器内 `/data`
- **THEN** `docker-compose.yml` 的顶级 `volumes` 中声明 `app-portal-data`

#### Scenario: 数据持久化
- **WHEN** 容器被停止并删除
- **THEN** `/data` 目录中的数据在 volume 中保留
- **WHEN** 新容器启动并挂载同一 volume
- **THEN** 应用可访问之前的数据

### Requirement: 容器重启策略
服务 SHALL 设置重启策略为 `unless-stopped`。

#### Scenario: 自动重启
- **WHEN** 容器因错误退出
- **THEN** Docker 自动重启容器
- **WHEN** 主机重启
- **THEN** Docker 自动启动容器（除非被手动停止）
