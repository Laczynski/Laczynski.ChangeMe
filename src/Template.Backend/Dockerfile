# Zobacz https://aka.ms/customizecontainer — Visual Studio (tryb szybki) używa stage „base”.
# Build z root repozytorium:
#   docker build -f src/Template.Backend/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY src/Template.Backend/ ./src/Template.Backend/
WORKDIR /src/src/Template.Backend/
RUN dotnet restore Template.Backend.slnx
RUN dotnet build src/Template.Backend.Web/Template.Backend.Web.csproj \
    -c "$BUILD_CONFIGURATION" \
    -o /app/build \
    --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish src/Template.Backend.Web/Template.Backend.Web.csproj \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

FROM base AS final
USER root
WORKDIR /app
COPY --from=publish --chown=$APP_UID:$APP_UID /app/publish .
RUN mkdir -p /app/storage && chown -R $APP_UID:$APP_UID /app/storage
USER $APP_UID
ENTRYPOINT ["dotnet", "Template.Backend.Web.dll"]
