using Connecty.Application.data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Connecty.Api.Health;

public class DatabaseHealthcheck : IHealthCheck
{
    public const string Service = "Postgres_Database";
    private readonly IDatabaseConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseHealthcheck> _logger;
    
    public DatabaseHealthcheck(IDatabaseConnectionFactory connectionFactory, ILogger<DatabaseHealthcheck> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            _ = await _connectionFactory.CreateConnectionAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            const string errorMessage = "Database is unhealthy";
            _logger.LogError(errorMessage);
            return HealthCheckResult.Unhealthy(errorMessage);
        }
    }
}