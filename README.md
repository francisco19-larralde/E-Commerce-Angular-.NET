# Ecommerce full-stack

Tienda online desarrollada con Angular y ASP.NET Core. Incluye catálogo, búsqueda y filtros, variantes por talle, carrito, checkout simulado, historial de compras y un panel administrativo con estadísticas.

> El pago es una simulación educativa. La aplicación no se conecta con una procesadora y no debe usarse con datos de tarjetas reales.

## Stack

- Frontend: Angular 20, TypeScript, RxJS, Tailwind CSS, DaisyUI y Chart.js.
- Backend: .NET 10, ASP.NET Core Web API, Entity Framework Core, Identity y SQL Server.
- Seguridad: JWT y autorización por roles `Cliente` y `Admin`.

## Funcionalidades

- Registro e inicio de sesión.
- Catálogo paginado con búsqueda, categorías, filtros y variantes.
- Carrito persistente por usuario con validación de stock.
- Checkout transaccional con cupones y simulación de pago.
- Historial y detalle de órdenes.
- Administración de productos, imágenes, categorías, variantes y stock.
- Dashboard de estadísticas mediante Chart.js.

## Estructura

```text
Backend/Ecommerce.Api/          API, servicios, entidades y migraciones
Backend/Ecommerce.Api.Tests/    Pruebas del backend
Frontend/ecommerce-app/         Aplicación Angular
.github/workflows/ci.yml        Integración continua
```

## Requisitos

- .NET SDK 10
- Node.js 22
- SQL Server o SQL Server LocalDB

No es necesario instalar Angular CLI globalmente: los scripts usan la versión local del proyecto.

## Configuración local

1. Copiá `Backend/Ecommerce.Api/appsettings.Example.json` como `Backend/Ecommerce.Api/appsettings.Development.json`.
2. Reemplazá la clave JWT del archivo local por una clave aleatoria de al menos 32 caracteres.
3. Opcionalmente, configurá el administrador inicial con User Secrets:

   ```bash
   dotnet user-secrets set "AdminSeed:Email" "admin@ecommerce.local" --project Backend/Ecommerce.Api
   dotnet user-secrets set "AdminSeed:Password" "AdminLocal1!" --project Backend/Ecommerce.Api
   ```

4. Aplicá las migraciones:

   ```bash
   dotnet ef database update --project Backend/Ecommerce.Api
   ```

## Ejecución

Iniciá la API:

```bash
dotnet run --project Backend/Ecommerce.Api
```

En otra terminal, iniciá Angular:

```bash
cd Frontend/ecommerce-app
npm ci
npm start
```

En desarrollo, el frontend utiliza `http://localhost:4200` y consume la API en `http://localhost:5000/api`.

### Configuración preparada para otro entorno

El backend permite configurar los orígenes CORS con variables de entorno. Por ejemplo:

```text
Cors__AllowedOrigins__0=https://frontend.example.com
Cors__AllowedOrigins__1=https://admin.example.com
```

Angular permite reemplazar la URL de la API después del build mediante `app-config.js`. Para generar el artefacto configurado:

```bash
cd Frontend/ecommerce-app
ECOMMERCE_API_URL=https://api.example.com/api npm run build:configured
```

En PowerShell:

```powershell
$env:ECOMMERCE_API_URL='https://api.example.com/api'
npm run build:configured
```

Si no se genera una configuración runtime, se mantiene el valor de `environment.ts` o `environment.development.ts`.

## Verificación

Backend:

```bash
dotnet build Backend/Ecommerce.Api/Ecommerce.Api.csproj --configuration Release
dotnet test Backend/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --configuration Release
```

Frontend:

```bash
cd Frontend/ecommerce-app
npm run build
npm run test:ci
```

## Decisiones técnicas

- El checkout usa una transacción `Serializable` para validar y descontar stock, registrar la orden, aplicar el cupón y vaciar el carrito de forma atómica.
- Las órdenes conservan una copia del nombre, talle y precio comprado para que el historial no cambie al editar el catálogo.
- Los controladores trabajan con DTOs y delegan las reglas de negocio a servicios.
- Las rutas Angular se cargan de forma diferida para reducir el bundle inicial.
- La sesión descarta tokens vencidos automáticamente y se cierra ante respuestas `401` de la API.
- Login y registro tienen rate limiting; los intentos fallidos bloquean temporalmente la cuenta.
- Las excepciones y errores HTTP sin cuerpo se devuelven como `ProblemDetails` con un `traceId`.
- `/health/live` informa si el proceso responde y `/health/ready` comprueba la conexión a la base.
- Producción emite logs JSON estructurados; desarrollo usa una consola legible de una sola línea.
- La base aplica índices únicos y constraints para proteger stock, cantidades, cupones y carritos.
- Las imágenes usan `IAlmacenamientoImagenes`; desarrollo registra el proveedor local y un proveedor externo puede reemplazarlo sin cambiar la lógica de productos.
- Los secretos y los archivos de configuración locales están excluidos del repositorio.

## Antes de un despliegue real

- Configurar la URL pública del frontend y la API, junto con CORS.
- Aplicar las migraciones como parte controlada del despliegue.
- Mover las imágenes a almacenamiento persistente de objetos.
- Sustituir el checkout simulado por un proveedor de pagos con tokenización.
- Configurar observabilidad, backups y health checks.

El procedimiento completo para crear, revisar, aplicar y revertir cambios de esquema está en [docs/MIGRACIONES.md](docs/MIGRACIONES.md).
