using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Adform.Bloom.Read.Application.Abstractions.Persistence;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Infrastructure.Configuration;
using Adform.Bloom.Read.Infrastructure.Decorators;
using Adform.Bloom.Read.Infrastructure.Metrics;
using Adform.Bloom.Read.Infrastructure.Repository;
using Adform.Ciam.Monitoring.Abstractions.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Adform.Bloom.Read.Infrastructure.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfigurationRoot configuration)
    {
        services.ConfigurationNpgSql(configuration);

        services.AddScoped<IRepository<User, UserWithCount>, UserRepository>();
        services.Decorate<IRepository<User, UserWithCount>>((inner, sp) =>
            new MeasuredRepository<User, UserWithCount>(inner,
                sp.GetRequiredService<IMetricsProvider>().GetHistogram(CommonMetrics.PsgqlDuration.Name)));

        services.AddScoped<IRepository<BusinessAccount, BusinessAccountWithCount>, BusinessAccountRepository>();
        services.Decorate<IRepository<BusinessAccount, BusinessAccountWithCount>>((inner, sp) =>
            new MeasuredRepository<BusinessAccount, BusinessAccountWithCount>(inner,
                sp.GetRequiredService<IMetricsProvider>().GetHistogram(CommonMetrics.PsgqlDuration.Name)));
        return services;
    }

    public static IServiceCollection ConfigurationNpgSql(this IServiceCollection services,
        IConfigurationRoot configuration)
    {
        var options = configuration.GetSection("NpgSql").Get<NpgSqlConfiguration>();
        return services.ConfigurationNpgSql(o =>
        {
            o.Host = options.Host;
            o.Port = options.Port;
            o.UserName = options.UserName;
            o.Password = options.Password;
            o.Database = options.Database;
        });
    }
    public static IServiceCollection ConfigurationNpgSql(this IServiceCollection services,
        Action<NpgSqlConfiguration> options)
    {
        services.Configure(options);
        services.AddTransient<IDbConnection>(sp =>
        {
            var configuration = sp.GetRequiredService<IOptions<NpgSqlConfiguration>>().Value;
            return new NpgsqlConnection(configuration.ConnectionString());
        });
        return services;
    }
}