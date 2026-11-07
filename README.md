# Gestión de Productos

Aplicación web con inicio de sesión seguro y gestión de productos (listar, crear, editar y eliminar),
con permisos según el rol del usuario.

- **Backend:** .NET 10 (ASP.NET Core Web API) + Entity Framework Core
- **Frontend:** React 19 + Vite + React Router
- **Base de datos:** SQL Server
- **Pruebas:** xUnit, contra la API real y SQL Server

## Estructura

```
backend/
  GestionProductos.Api/     API: controladores, autenticación, acceso a datos
  GestionProductos.Tests/   Pruebas de autenticación y del CRUD de productos
frontend/                   Aplicación en React
database/script.sql         Script de creación de la base de datos, con datos iniciales
docker/                     Preparación de la base en Docker
docker-compose.yml          Levanta todo con un solo comando
```

## Usuarios de prueba

Los crea `database/script.sql`.

| Correo | Contraseña | Rol | Puede |
|---|---|---|---|
| admin@demo.com | `Admin123!` | Admin | Ver, crear, editar y eliminar productos |
| usuario@demo.com | `Usuario123!` | User | Solo ver productos |

---

## Opción 1: con Docker (recomendada)

Solo necesitas **Docker Desktop**. Levanta SQL Server, la API y el frontend, y crea la base sola.

```bash
cp .env.example .env
```

Abre `.env` y cambia las dos contraseñas y la clave del token (el archivo explica los requisitos).
Después:

```bash
docker compose up --build
```

La primera vez tarda unos minutos, porque descarga las imágenes. Cuando termine:

| Qué | Dirección |
|---|---|
| Aplicación | http://localhost:8080 |
| API | http://localhost:5142/api |
| Estado de la API y la base | http://localhost:5142/api/health |

- Los datos se guardan en un volumen de Docker y se conservan entre reinicios.
- Para apagar: `docker compose down`. Para borrar también los datos: `docker compose down -v`.
- Los puertos 8080, 5142 y 1433 deben estar libres.

## Opción 2: en local, sin Docker

### Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20 o superior](https://nodejs.org)
- SQL Server: **LocalDB** (viene con Visual Studio) u otra instancia de SQL Server
- `sqlcmd`, o SQL Server Management Studio para ejecutar el script

### 1. Base de datos

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -f 65001 -i database/script.sql
```

El script crea la base `GestionProductos` con sus tablas y los datos iniciales. Si la base ya
existe, la borra y la vuelve a crear, así que sirve también para empezar de cero.

Para otro SQL Server, cambia `ConnectionStrings:DefaultConnection` en
`backend/GestionProductos.Api/appsettings.json`.

### 2. Backend

```bash
dotnet run --project backend/GestionProductos.Api --launch-profile http
```

Queda en http://localhost:5142. En desarrollo, la clave del token está en
`appsettings.Development.json`; en cualquier otro entorno hay que darla con la variable
`Jwt__Key` (32 caracteres como mínimo), o la API no arranca.

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

Abre http://localhost:5173. Si la API está en otra dirección, copia `frontend/.env.example`
como `frontend/.env` y cambia `VITE_API_URL`.

---

## Pruebas

```bash
dotnet test backend
```

Son 28 pruebas de integración: levantan la API completa en memoria y la prueban contra
SQL Server real. Cada ejecución crea su propia base con `database/script.sql` y la borra al
terminar, así que no tocan tus datos.

| Archivo | Qué cubre |
|---|---|
| `AuthTests.cs` | Login correcto e incorrecto, mismo error para correo inexistente, validaciones, `/me` con y sin token, token alterado o firmado con otra clave, límite de intentos (429) |
| `ProductsTests.cs` | Acceso sin token (401), usuario sin permiso para escribir (403), crear, consultar, editar y eliminar como Admin, 404, validaciones de precio, stock, nombre y tipo, JSON mal formado, tipos de producto |

Por defecto usan LocalDB. Para usar otro servidor, por ejemplo el de Docker, se indica con
`TEST_SQLSERVER`:

```bash
TEST_SQLSERVER="Server=127.0.0.1,1433;User Id=sa;Password=<MSSQL_SA_PASSWORD>;TrustServerCertificate=True" dotnet test backend
```

(Con Docker, usa `127.0.0.1` y no `localhost`: el puerto solo se publica en IPv4.)

## API

Todas las rutas, salvo el login y `/health`, exigen el token en la cabecera
`Authorization: Bearer <token>`.

| Método | Ruta | Quién | Qué hace |
|---|---|---|---|
| POST | `/api/auth/login` | Cualquiera | Recibe correo y contraseña; devuelve el token JWT |
| GET | `/api/auth/me` | Con sesión | Datos del dueño del token |
| GET | `/api/products` | Con sesión | Lista de productos |
| GET | `/api/products/{id}` | Con sesión | Un producto |
| POST | `/api/products` | Admin | Crea un producto |
| PUT | `/api/products/{id}` | Admin | Edita un producto |
| DELETE | `/api/products/{id}` | Admin | Elimina un producto |
| GET | `/api/product-types` | Con sesión | Tipos de producto |
| GET | `/api/health` | Cualquiera | Estado de la API y de la conexión a la base |

Los errores siguen el formato ProblemDetails (RFC 9457), con el título en español.

## Seguridad

**Backend**
- Contraseñas guardadas solo como hash **BCrypt** (costo 11, con sal propia por usuario).
- **JWT** firmado con HMAC-SHA256: se validan la firma, el emisor, el destinatario y el vencimiento (60 minutos).
- Autorización **por rol** en cada endpoint: un usuario sin rol Admin recibe 403 al intentar escribir.
- El login responde igual, y tarda lo mismo, si el correo no existe o si la contraseña es incorrecta: no revela qué correos están registrados.
- Máximo **5 intentos de login por minuto** por IP (429), contra la fuerza bruta.
- **CORS** solo para el origen del frontend.
- Un error inesperado no expone detalles internos: devuelve un código para buscarlo en los logs.
- La clave del token no está en `appsettings.json`; fuera de desarrollo llega por variable de entorno.
- Todas las consultas pasan por EF Core, con parámetros: no hay SQL armado con texto.

**Frontend**
- El token se guarda en `localStorage` y se envía en cada petición; si vence o el backend lo rechaza, se cierra la sesión.
- Rutas protegidas: sin sesión, todo redirige al login; las pantallas de crear y editar exigen el rol Admin.
- Validación de todos los campos antes de enviar, con las mismas reglas que el backend.

**Docker**
- La API se conecta con un usuario de SQL Server que solo puede leer y escribir datos, no `sa`.
- Contraseñas y claves en `.env`, que no se sube al repositorio.
- Los contenedores de la API y del frontend corren sin privilegios de administrador.
- nginx envía cabeceras de seguridad, entre ellas una **Content-Security-Policy** que solo permite código del propio sitio y llamadas a la API.
- SQL Server solo se publica en `127.0.0.1`, no en la red.
