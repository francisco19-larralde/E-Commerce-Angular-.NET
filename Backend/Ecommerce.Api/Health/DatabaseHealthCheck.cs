using Ecommerce.Api.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ecommerce.Api.Health;

public sealed class DatabaseHealthCheck(AppDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("La base de datos está disponible.")
                : HealthCheckResult.Unhealthy("No se pudo conectar con la base de datos.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Falló la comprobación de la base de datos.",
                exception);
        }
    }
}
