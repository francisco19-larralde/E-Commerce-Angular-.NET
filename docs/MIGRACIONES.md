# Migraciones de base de datos

Las migraciones de Entity Framework son parte del código fuente. La API no las aplica automáticamente al arrancar: deben ejecutarse como un paso controlado antes de iniciar una versión nueva.

## Crear una migración

Desde la raíz del repositorio:

```bash
dotnet ef migrations add NombreDescriptivo --project Backend/Ecommerce.Api --startup-project Backend/Ecommerce.Api
```

Después de generarla:

1. Revisá los métodos `Up` y `Down`.
2. Confirmá que no haya `DropTable` o `DropColumn` inesperados.
3. Prestá atención a cambios de longitud, índices únicos y columnas no anulables: pueden fallar si los datos existentes no cumplen la nueva regla.
4. Ejecutá los tests del backend.

## Probarla en desarrollo

Hacé un backup si la base contiene datos importantes y ejecutá:

```bash
dotnet ef database update --project Backend/Ecommerce.Api --startup-project Backend/Ecommerce.Api
```

Comprobá luego el arranque de la API y los flujos afectados.

## Generar SQL para revisión

Para revisar o entregar un script idempotente:

```bash
dotnet ef migrations script --idempotent --project Backend/Ecommerce.Api --startup-project Backend/Ecommerce.Api --output migracion.sql
```

El script idempotente consulta `__EFMigrationsHistory` y sólo aplica migraciones pendientes. En producción debe ejecutarlo el pipeline o una tarea de release con credenciales limitadas y después de crear un backup.

## Verificar cambios de modelo pendientes

Antes de cerrar una tarea que modifica entidades o `AppDbContext`:

```bash
dotnet ef migrations has-pending-model-changes --project Backend/Ecommerce.Api --startup-project Backend/Ecommerce.Api
```

Si el comando informa cambios pendientes, generá y revisá una migración antes de publicar.

## Revertir

Para volver a una migración anterior en desarrollo:

```bash
dotnet ef database update NombreMigracionAnterior --project Backend/Ecommerce.Api --startup-project Backend/Ecommerce.Api
```

Revertir puede eliminar datos. En producción se recomienda una migración correctiva hacia adelante; sólo debe hacerse rollback directo después de revisar el método `Down` y contar con un backup restaurable.

## Configuración

La conexión se obtiene de `ConnectionStrings:DefaultConnection`. En un entorno desplegado debe proporcionarse como secreto:

```text
ConnectionStrings__DefaultConnection=...
```

No guardes cadenas de conexión reales en `appsettings.json`, scripts SQL ni archivos versionados.
