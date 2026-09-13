# Build from the repository root:
#   docker build -f src/CRISP.Server/Dockerfile -t crisp-server .
#   docker build -f src/CRISP.Server/Dockerfile --target migrator -t crisp-migrator .
# Central package management (Directory.Packages.props), the shared build properties and the SDK pin
# (global.json) live at the root and must sit above the project folders for restore to find them.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
#if (IncludeReact)
# The Server references the Client esproj, and the JavaScript SDK needs Node and node_modules to
# evaluate it. Node 24 matches src/CRISP.Client/.nvmrc.
RUN curl -fsSL https://deb.nodesource.com/setup_24.x | bash - && apt-get install -y --no-install-recommends nodejs && rm -rf /var/lib/apt/lists/*
#endif
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Packages.props", "global.json", "./"]
COPY ["src/CRISP.Core/CRISP.Core.csproj", "src/CRISP.Core/"]
COPY ["src/CRISP.Server/CRISP.Server.csproj", "src/CRISP.Server/"]
COPY ["src/CRISP.Migrator/CRISP.Migrator.csproj", "src/CRISP.Migrator/"]
#if (UseAspire)
COPY ["src/CRISP.ServiceDefaults/CRISP.ServiceDefaults.csproj", "src/CRISP.ServiceDefaults/"]
#endif
#if (IncludeReact)
COPY ["src/CRISP.Client/CRISP.Client.esproj", "src/CRISP.Client/package.json", "src/CRISP.Client/package-lock.json", "src/CRISP.Client/"]
RUN cd src/CRISP.Client && npm ci --no-audit --no-fund
#endif
RUN dotnet restore src/CRISP.Server/CRISP.Server.csproj && dotnet restore src/CRISP.Migrator/CRISP.Migrator.csproj
COPY src/ src/
#if (IncludeReact)
# Building Core runs TypeGen into src/CRISP.Client/src/api, which the client needs to compile.
RUN dotnet build src/CRISP.Core/CRISP.Core.csproj -c $BUILD_CONFIGURATION --no-restore
RUN cd src/CRISP.Client && npm run build
#endif
RUN dotnet publish src/CRISP.Server/CRISP.Server.csproj -c $BUILD_CONFIGURATION --no-restore -o /app/server /p:UseAppHost=false
RUN dotnet publish src/CRISP.Migrator/CRISP.Migrator.csproj -c $BUILD_CONFIGURATION --no-restore -o /app/migrator /p:UseAppHost=false

# Run before each rollout; exit 0 means the schema is current. See docs/operations/migrations.md.
# The Migrator references the Server for the DbContext, so it needs the ASP.NET shared framework.
FROM base AS migrator
COPY --from=build /app/migrator .
ENTRYPOINT ["dotnet", "CRISP.Migrator.dll"]

FROM base AS final
COPY --from=build /app/server .
#if (IncludeReact)
COPY --from=build /src/src/CRISP.Client/dist/ ./wwwroot/
#endif
ENTRYPOINT ["dotnet", "CRISP.Server.dll"]
