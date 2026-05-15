FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY AppPortal/AppPortal.csproj .
RUN dotnet restore
COPY AppPortal/ .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV LiteDb__Path=/data/AppPortal.db
VOLUME /data
ENTRYPOINT ["dotnet", "AppPortal.dll"]
