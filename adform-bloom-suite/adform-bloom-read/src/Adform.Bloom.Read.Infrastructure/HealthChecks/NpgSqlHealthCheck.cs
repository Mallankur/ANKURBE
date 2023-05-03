using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Adform.Bloom.Read.Infrastructure.HealthChecks;

public static class Tags
{
    public const string NpgSql = "npgsql";
}
public static class Paths
{
    public const string Configuration = "NpgSql";
}

public class NpgSqlHealthCheck
    : IHealthCheck
{
    private readonly ILogger<NpgSqlHealthCheck> _logger;
    private readonly IDbConnection _connection;

    public NpgSqlHealthCheck(ILogger<NpgSqlHealthCheck> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                command.ExecuteScalar();
            }

            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
        }
    }
}