# Build context is the repository root (see AINews.API.csproj DockerfileContext).
# Build with:  docker build -f src/AINews.API/Dockerfile -t ainews-api .
# Or simply:   docker compose up --build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/AINews.Domain/AINews.Domain.csproj", "src/AINews.Domain/"]
COPY ["src/AINews.Application/AINews.Application.csproj", "src/AINews.Application/"]
COPY ["src/AINews.Infrastructure/AINews.Infrastructure.csproj", "src/AINews.Infrastructure/"]
COPY ["src/AINews.API/AINews.API.csproj", "src/AINews.API/"]
RUN dotnet restore "src/AINews.API/AINews.API.csproj"

COPY . .
WORKDIR "/src/src/AINews.API"
RUN dotnet build "AINews.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AINews.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AINews.API.dll"]
