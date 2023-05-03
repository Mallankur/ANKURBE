using Adform.Ciam.SharedKernel.Configuration;
using Microsoft.Extensions.Options;

namespace Adform.Bloom.Read.Infrastructure.Configuration;

public class NpgSqlConfiguration : IValidatableConfiguration
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Database { get; set; } = "default";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrEmpty(Host))
            throw new OptionsValidationException(nameof(Host), typeof(NpgSqlConfiguration),
                new[] { $"{nameof(Host)} cannot be empty!" });

        if (string.IsNullOrEmpty(Database))
            throw new OptionsValidationException(nameof(Database), typeof(NpgSqlConfiguration),
                new[] { $"{nameof(Database)} cannot be empty!" });

        if (string.IsNullOrEmpty(UserName))
            throw new OptionsValidationException(nameof(UserName), typeof(NpgSqlConfiguration),
                new[] { $"{nameof(UserName)} cannot be empty!" });

        if (string.IsNullOrEmpty(Password))
            throw new OptionsValidationException(nameof(Password), typeof(NpgSqlConfiguration),
                new[] { $"{nameof(Password)} cannot be empty!" });

        if (Port < 0)
            throw new OptionsValidationException(nameof(Port), typeof(NpgSqlConfiguration),
                new[] { $"{nameof(Port)} cannot be negative!" });
    }
}