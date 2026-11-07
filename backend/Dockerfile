# API de Gestión de Productos (.NET 10).

# ---------- Compilación ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo el .csproj: si no cambian los paquetes, Docker reutiliza la restauración.
COPY GestionProductos.Api/GestionProductos.Api.csproj GestionProductos.Api/
RUN dotnet restore GestionProductos.Api/GestionProductos.Api.csproj

COPY GestionProductos.Api/ GestionProductos.Api/
RUN dotnet publish GestionProductos.Api/GestionProductos.Api.csproj -c Release -o /app --no-restore

# ---------- Ejecución ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Sin privilegios de administrador dentro del contenedor.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "GestionProductos.Api.dll"]
