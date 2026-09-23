<<<<<<< HEAD
# Ecommerce full-stack

Tienda online desarrollada con Angular y ASP.NET Core. Incluye catálogo, búsqueda y filtros, variantes por talle, carrito, checkout simulado, historial de compras y un panel administrativo con estadísticas.

## Funcionalidades

- Registro e inicio de sesión con JWT y roles `Cliente`/`Admin`.
- Catálogo paginado, búsqueda, categorías y variantes de producto.
- Carrito persistente por usuario y validación de stock.
- Checkout transaccional con cupones y simulación de pago.
- Historial y detalle de órdenes.
- Administración de productos, imágenes, categorías, variantes y stock.
- Dashboard con estadísticas mediante Chart.js.

> El pago es una simulación educativa. La aplicación no se conecta con una procesadora ni almacena el número completo o CVV de la tarjeta; sólo conserva los últimos cuatro dígitos en la orden.

## Stack

- Frontend: Angular 20, TypeScript, RxJS, Tailwind CSS, DaisyUI y Chart.js.
- Backend: .NET 10, ASP.NET Core Web API, Entity Framework Core, Identity y SQL Server.
- Seguridad: JWT, autorización por roles y secretos fuera del repositorio.

## Decisiones técnicas

El checkout utiliza una transacción `Serializable`: la lectura del carrito, validación y descuento de stock, uso del cupón, creación de la orden y vaciado del carrito se confirman como una sola operación. Esto evita que dos compras concurrentes vendan las mismas unidades.

Los controladores trabajan con DTOs y delegan las reglas de negocio a servicios inyectados por interfaz.

## Ejecución local

1. Copiar `Backend/Ecommerce.Api/appsettings.Example.json` como `appsettings.Development.json` y reemplazar la clave JWT.
2. Ejecutar el backend:

   ```bash
   dotnet run --project Backend/Ecommerce.Api
   ```

3. En otra terminal, ejecutar el frontend:

   ```bash
   cd Frontend/ecommerce-app
   npm ci
   npm start
   ```

La API de desarrollo usa `http://localhost:5000` y el frontend `http://localhost:4200`.

## Próximos pasos de producción

- Reemplazar el simulador de pago por un proveedor con tokenización.
- Guardar imágenes en almacenamiento de objetos.
- Configurar URLs y secretos mediante variables del entorno de despliegue.
- Publicar capturas y credenciales de una cuenta demo sin privilegios sensibles.
=======
# E-commerce  — Angular + .NET

Proyecto de portfolio: una tienda online completa, construida desde cero para practicar el desarrollo full-stack con **Angular** en el frontend y **ASP.NET Core Web API** en el backend, siguiendo una arquitectura en capas y buenas prácticas de la industria.

Incluye catálogo con filtros, variantes de producto (talles), carrito de compras, checkout simulado con descuento de stock real, cupones de descuento, autenticación con JWT, y un panel de administración completo con estadísticas de ventas.

---

## Índice

- [Tecnologías](#tecnologías)
- [Funcionalidades](#funcionalidades)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Requisitos previos](#requisitos-previos)
- [Cómo levantar el proyecto](#cómo-levantar-el-proyecto)
- [Usuario administrador de prueba](#usuario-administrador-de-prueba)
- [Datos de prueba para el checkout](#datos-de-prueba-para-el-checkout)
- [Arquitectura y decisiones de diseño](#arquitectura-y-decisiones-de-diseño)

---

## Tecnologías

### Backend
- **.NET 10** — ASP.NET Core Web API
- **Entity Framework Core** — ORM, enfoque Code-First con Migrations
- **SQL Server / LocalDB** — base de datos relacional
- **ASP.NET Identity** — gestión de usuarios y roles
- **JWT (JSON Web Tokens)** — autenticación stateless
- **Swagger / Swashbuckle** — documentación y testing interactivo de la API

### Frontend
- **Angular 20** — standalone components, Signals, nueva sintaxis de control de flujo (`@if`, `@for`)
- **TypeScript**
- **TailwindCSS** + **DaisyUI** — estilos utility-first con componentes semánticos por tema
- **RxJS** — manejo de flujos asíncronos (búsqueda en vivo, sesión, etc.)
- **Chart.js** — gráficos del panel de estadísticas

### Arquitectura
- Backend organizado en capas: **Controllers → Services (con interfaces) → Entity Framework**
- Patrón **Result Pattern** para manejo de errores de negocio sin excepciones
- **DTOs** en toda la API (nunca se exponen las entidades de base de datos directamente)
- Frontend con **Services + Signals** para estado reactivo, **Guards** para protección de rutas, e **Interceptors** para autenticación y manejo global de errores

---

## Funcionalidades

### Tienda (usuario público / cliente)
- Catálogo de productos con destacados y carruseles por categoría en el Home
- Página de catálogo completo con filtros por categoría, precio, talle y búsqueda, con paginación
- Buscador en vivo en la navbar (debounce + cancelación de búsquedas anteriores)
- Detalle de producto con selector de talle (cuando aplica) y validación de stock en tiempo real
- Múltiples imágenes por producto con efecto hover tipo carrusel en las cards
- Registro e inicio de sesión con JWT, con expiración automática de sesión
- Carrito de compras persistente por usuario, con control de stock
- Checkout simulado: carga de tarjeta, aplicación de cupones de descuento, y descuento real de stock al confirmar la compra
- Historial de compras del usuario ("Mis compras") con detalle de cada orden

### Panel de administración (rol Admin)
- Layout propio con sidebar de navegación persistente (sin recargar la página al cambiar de sección)
- ABM de productos con paginación, búsqueda y filtro por categoría
- Edición rápida de "destacado" y "activo" directo desde la tabla
- Gestión de variantes (talles) por producto, con stock unificado automáticamente
- Subida real de imágenes de producto (no por URL, sino desde archivo)
- Aplicación de descuentos por producto (porcentaje), reflejado en toda la tienda
- Gestión de categorías, incluyendo cuáles se muestran como carrusel en el Home y en qué orden
- Dashboard de estadísticas: ingresos totales, cantidad de órdenes, ticket promedio, gráfico de ventas por día y ranking de productos más vendidos

---

## Estructura del proyecto

```
ecommerce-portfolio/
│
├── Backend/
│   └── Ecommerce.Api/
│       ├── Controllers/       # Endpoints de la API
│       ├── Services/          # Lógica de negocio (con sus interfaces)
│       ├── Models/            # Entidades de Entity Framework
│       ├── DTOs/              # Contratos de entrada/salida de la API
│       ├── Data/               # DbContext y seed de datos iniciales
│       └── Migrations/        # Historial de cambios de la base de datos
│
├── Frontend/
│   └── ecommerce-app/
│       └── src/app/
│           ├── pages/          # Componentes de página (una por ruta)
│           ├── components/     # Componentes reutilizables (cards, carruseles, etc.)
│           ├── services/       # Comunicación HTTP y estado compartido
│           ├── guards/         # Protección de rutas
│           ├── interceptors/   # Token JWT y manejo global de errores HTTP
│           └── models/         # Interfaces de TypeScript (espejo de los DTOs)
│
└── .gitattributes             # Normaliza finales de línea (LF) en todo el repositorio
```

---

## Requisitos previos

Para correr el proyecto en tu máquina necesitás tener instalado:

| Herramienta | Versión | Uso |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 o superior | Backend |
| [Node.js](https://nodejs.org) | 20 LTS o superior | Frontend |
| [Angular CLI](https://angular.dev/tools/cli) | 20 o superior | `npm install -g @angular/cli` |
| SQL Server LocalDB | (viene con Visual Studio / .NET SDK en Windows) | Base de datos |

---

## Cómo levantar el proyecto

### 1. Cloná el repositorio

```bash
git clone https://github.com/TU-USUARIO/ecommerce-portfolio.git
cd ecommerce-portfolio
```

### 2. Backend

```bash
cd Backend/Ecommerce.Api
```

Configurá las credenciales del usuario administrador de prueba (se usan solo para crearlo automáticamente al arrancar; no quedan en el código fuente):

```bash
dotnet user-secrets init
dotnet user-secrets set "AdminSeed:Email" "admin@ecommerce.com"
dotnet user-secrets set "AdminSeed:Password" "Admin123!"
```

Aplicá las migraciones para crear la base de datos:

```bash
dotnet ef database update
```

> Si no tenés instalada la herramienta de migraciones: `dotnet tool install --global dotnet-ef`

Corré el servidor:

```bash
dotnet run
```

La API queda disponible en `http://localhost:5000` (confirmá el puerto exacto en la consola), y la documentación interactiva en `http://localhost:5000/swagger`.

Al arrancar por primera vez, se crean automáticamente: los roles (`Admin`, `Cliente`), el usuario administrador con las credenciales configuradas arriba, un cupón de descuento de prueba (`BIENVENIDO10`, 10% off), y — solo en entorno de desarrollo — un catálogo inicial de categorías y productos de ejemplo.

### 3. Frontend

En otra terminal:

```bash
cd Frontend/ecommerce-app
npm install
ng serve
```

La aplicación queda disponible en `http://localhost:4200`.

> Si tu backend corre en un puerto distinto a 5000, ajustá `apiUrl` en `src/environments/environment.development.ts`.

### 4. Listo

Con ambos procesos corriendo en paralelo, entrá a `http://localhost:4200` en el navegador.

---

## Usuario administrador de prueba

Con las credenciales configuradas en el paso de instalación (o las que vos mismo definas):

```
Email:    admin@ecommerce.com
Password: Admin123!
```

Accedé al panel desde el link "Panel Admin" en la navbar tras iniciar sesión, o directamente en `/admin`.

## Datos de prueba para el checkout

El checkout es una **simulación de pago**, no procesa cobros reales:

- Cualquier número de tarjeta válido (por ejemplo `4111 1111 1111 1111`) simula un pago aprobado.
- Un número de tarjeta terminado en `0000` simula un pago **rechazado**, para poder probar ese flujo.
- Cupón de descuento de prueba: `BIENVENIDO10` (10% de descuento).

---

## Arquitectura y decisiones de diseño

Algunas decisiones técnicas tomadas a lo largo del desarrollo, documentadas para quien revise el código:

- **DTOs en toda la API**: las entidades de Entity Framework nunca se exponen directamente en las respuestas HTTP, para desacoplar el modelo de base de datos del contrato público de la API.
- **Arquitectura en capas**: los Controllers no acceden a la base de datos ni contienen lógica de negocio — esa responsabilidad vive en los Services, inyectados por interfaz.
- **Result Pattern**: los errores de negocio esperables (stock insuficiente, cupón inválido, etc.) se modelan como valores de retorno explícitos, reservando las excepciones de C# para casos verdaderamente excepcionales.
- **Soft delete de productos**: los productos nunca se borran físicamente si ya tuvieron actividad; se marcan como inactivos para preservar la integridad de compras ya realizadas.
- **Snapshots en las órdenes**: cada compra guarda una copia congelada del nombre, talle y precio de cada producto al momento de comprar, para que cambios futuros en el catálogo no alteren el historial de compras.
- **Stock unificado**: cuando un producto tiene variantes (talles), el stock general se calcula automáticamente como la suma de sus talles, evitando inconsistencias entre ambos valores.
- **Sesión con expiración real**: el token JWT se invalida automáticamente en el frontend al vencer, sin depender de que el usuario dispare una petición fallida para notarlo.
- **Secretos fuera del código**: credenciales sensibles (como el usuario administrador de prueba) se gestionan con User Secrets en desarrollo, nunca hardcodeadas en el repositorio.

---

>>>>>>> 2d6281a7f4f02b90e8f991b1bc8751195c98788b
