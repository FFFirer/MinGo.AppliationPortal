## 1. 项目文件修改

- [x] 1.1 修改 AppPortal.csproj：TailwindBuild Target 增加 `And !Exists('wwwroot/css/app.css')` 条件，当 CSS 文件已存在时跳过执行

## 2. Dockerfile

- [x] 2.1 创建 `Dockerfile`，实现多阶段构建（SDK restore → SDK publish → ASP.NET runtime）
- [x] 2.2 配置容器环境变量：`ASPNETCORE_URLS=http://+:8080`、`LiteDb__Path=/data/AppPortal.db`
- [x] 2.3 声明 VOLUME `/data` 用于 LiteDB 持久化
- [ ] 2.4 验证 local build：`docker build -t portal-test . && docker run -p 8080:8080 portal-test` 可正常访问

## 3. Gitea Actions 工作流

- [x] 3.1 创建 `.gitea/workflows/` 目录
- [x] 3.2 创建 `docker-publish.yml`，定义 `on: push: tags: v*.*.*` 和 `workflow_dispatch` 触发器
- [x] 3.3 添加 Tailwind CSS 预编译步骤（setup-node → npm ci → npm run css:prod）
- [x] 3.4 添加 Docker BuildX 初始化步骤（setup-buildx-action + setup-qemu-action）
- [x] 3.5 添加私有 Registry 登录步骤（docker/login-action，使用 Secrets）
- [x] 3.6 添加 metadata 步骤（docker/metadata-action，version/major.minor/major/latest 标签）
- [x] 3.7 添加构建并推送步骤（docker/build-push-action + registry cache）
- [x] 3.8 检查：如果私有 Registry 使用 HTTP，配置 BuildX 允许 insecure registry

## 4. Docker Compose 部署配置

- [x] 4.1 创建 `docker-compose.yml`，定义 app-portal 服务
- [x] 4.2 配置端口映射 `8080:8080`
- [x] 4.3 配置环境变量 `ASPNETCORE_ENVIRONMENT=Production` 和 `LiteDb__Path=/data/AppPortal.db`
- [x] 4.4 定义命名 volume `app-portal-data` 挂载到 `/data`
- [x] 4.5 设置重启策略 `unless-stopped`
- [ ] 4.6 验证：`docker compose up -d` 启动后应用可正常访问，数据持久化正常

## 5. 验证与文档

- [ ] 5.1 在 Gitea 仓库设置中创建所需 Secrets（REGISTRY_URL、REGISTRY_USER、REGISTRY_PASSWORD）
- [ ] 5.2 验证工作流：推送测试 tag 触发构建，确认镜像推送到私有 Registry
- [ ] 5.3 确认镜像标签正确（version / major.minor / major / latest）
- [ ] 5.4 确认 Docker Compose 拉取最新镜像并正常启动
