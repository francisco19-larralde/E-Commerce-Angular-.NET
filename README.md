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
