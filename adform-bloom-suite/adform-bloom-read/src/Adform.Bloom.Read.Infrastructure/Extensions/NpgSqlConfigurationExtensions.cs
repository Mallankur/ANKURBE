using System.Linq;
using Adform.Bloom.Read.Infrastructure.Configuration;

namespace Adform.Bloom.Read.Infrastructure.Extensions;

public static class NpgSqlConfigurationExtensions
{
    public static string ConnectionString(this NpgSqlConfiguration options)
    {
        return $"Host={options.Host};User ID={options.UserName};Password={options.Password};Port={options.Port};Database={options.Database};Pooling=true;";
    }
        
    public static string ToPropertyOrDefault<T>(this string str)
    {
        return typeof(T).GetProperties()
            .Select(prop => prop.Name).ToList()
            .Contains(str) ? str : "Id";
    }
}