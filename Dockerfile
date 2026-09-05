FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY TaskManager.Api.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
