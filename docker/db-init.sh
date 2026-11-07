#!/bin/bash
# Prepara la base de datos en el contenedor de SQL Server. Corre una vez en cada `docker compose up`.
#   1. Si la base no existe, la crea con database/script.sql (si ya existe, no la toca: tus datos se conservan).
#   2. Crea o actualiza el usuario con el que se conecta la API, que solo puede leer y escribir datos.
set -euo pipefail

SQLCMD=(/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -b)

exists=$("${SQLCMD[@]}" -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'GestionProductos'")

if [ "$exists" = "0" ]; then
    echo "Creando la base GestionProductos..."
    "${SQLCMD[@]}" -f 65001 -i /scripts/script.sql
else
    echo "La base GestionProductos ya existe; no se modifica."
fi

echo "Configurando el usuario de la aplicación..."
"${SQLCMD[@]}" -v APP_PASSWORD="$APP_DB_PASSWORD" -i /scripts/app-user.sql
echo "Base de datos lista."
