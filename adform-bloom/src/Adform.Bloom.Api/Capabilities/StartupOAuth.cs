using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Adform.Ciam.Authentication.Configuration;
using Adform.Ciam.Authentication.Extensions;
using Adform.Ciam.Authorization.Extensions;

namespace Adform.Bloom.Api.Capabilities
{
    public static class StartupOAuth
    {
        public static IServiceCollection ConfigureOAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var oauthConfig = configuration.GetSection("OAuth").Get<AuthConfiguration>();
            oauthConfig.Mode = new[] {AuthMode.ClientCredentials};
            services.ConfigureAuthentication(p =>
            {
                p.Authority = oauthConfig.Authority;
                p.Audience = oauthConfig.Audience;
                p.ClientId = oauthConfig.ClientId;
                p.ClientSecret = oauthConfig.ClientSecret;
                p.Scopes = oauthConfig.Scopes;
                p.Mode = oauthConfig.Mode;
                p.UseHttps = oauthConfig.UseHttps;
            });
            return services;
        }
        
        public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddScopePolicy(Scopes.Readonly);
                options.AddScopePolicy(Scopes.Full);
                options.AddScopePolicy(Scopes.FullSubject);
                options.AddScopePolicy(Scopes.Readonly,Scopes.Full);
                options.AddScopePolicy(Scopes.Readonly, Scopes.Full, Scopes.FullSubject);
            });
            return services;
        }

        public static class Scopes
        {
            public const string Readonly = "https://api.adform.com/scope/bloom.management.readonly";
            public const string Full = "https://api.adform.com/scope/bloom.management";
            public const string FullSubject = "https://api.adform.com/scope/bloom.management.subject";
        }

        public static class Permissions
        {
            public const string SubjectCanRead = "CIAM.Subject.CanRead";
            public const string SubjectCanManage = "CIAM.Subject.CanManage";
            public const string SubjectCanAssign = "CIAM.Subject.CanAssign";
        }
    }
}