## 1. Dockerfile — 重构为 3-stage

- [x] 1.1 新增 frontend-build 阶段：使用 `node:22-alpine`，复制 `package.json` 和 `package-lock.json`，执行 `npm ci`
- [x] 1.2 frontend-build 阶段：复制 `AppPortal/` 全部前端文件，执行 `npm run css:prod` 编译 Tailwind CSS
- [x] 1.3 修改 build 阶段：从 frontend-build 阶段复制 `wwwroot/css/app.css` 到工作目录供 dotnet publish 使用
- [x] 1.4 修改 final 阶段：确认 EXPOSE / ENV / VOLUME / ENTRYPOINT 保持不变
- [ ] 1.5 验证：`docker build` 成功，镜像包含编译后的 CSS，运行时能正常访问

## 2. CI Workflow — 简化流程

- [x] 2.1 移除 `actions/setup-node`、`npm ci`、`npm run css:prod` 等前端构建步骤
- [x] 2.2 新增 `env` 块：`CONTAINER_REGISTRY`、`CONTAINER_REGISTRY_USERNAME`、`CONTAINER_REGISTRY_PASSWORD`、`IMAGE_NAME`（对齐 ReverseProxy 模式）
- [x] 2.3 Registry 登录步骤改为使用 `vars.DOCKER_REGISTRY` / `vars.DOCKER_USERNAME` / `secrets.DOCKER_PASSWORD`
- [x] 2.4 镜像标签改为使用 `env.CONTAINER_REGISTRY` 和 `env.IMAGE_NAME` 拼接（保留 `docker/metadata-action` 和 build cache）

## 3. docker-compose.yml — 镜像命名对齐

- [x] 3.1 更新 `image` 字段为可被 CI 环境变量覆盖的形式（对齐新 Registry 配置模式）

## 4. 更新 OpenSpec Specs

- [x] 4.1 将 container-image delta spec 同步到主 spec：移除 "不包含 Node.js/Tailwind 构建" 需求，修改多阶段构建场景
- [x] 4.2 将 gitea-cd delta spec 同步到主 spec：移除 "Tailwind CSS 预编译" 需求，修改 Registry 登录场景
