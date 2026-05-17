# 第一阶段：构建前端 (Tailwind CSS)
FROM swr.cn-north-4.myhuaweicloud.com/ddn-k8s/docker.io/library/node:20-alpine AS frontend-build
WORKDIR /app

# 复制依赖文件，利用 Docker 层缓存
COPY AppPortal/package.json AppPortal/package-lock.json ./
RUN npm ci

# 复制前端资源并编译
COPY AppPortal/ .
RUN npm run css:prod

# 第二阶段：构建 .NET 应用
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制项目文件，利用 Docker 层缓存
COPY AppPortal/AppPortal.csproj .
RUN dotnet restore

# 复制全部项目文件
COPY AppPortal/ .

# 从前端构建阶段复制预编译的 CSS
COPY --from=frontend-build /app/wwwroot/css/app.css ./wwwroot/css/app.css

# 发布 Web 站点
RUN dotnet publish -c Release -o /app/publish

# 第三阶段：发布
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV LiteDb__Path=/data/AppPortal.db
VOLUME /data
ENTRYPOINT ["dotnet", "AppPortal.dll"]
