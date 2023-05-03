using Adform.Ciam.Swagger.Extensions;
using Adform.Ciam.Swagger.Options;
using Adform.Ciam.Swagger.Schemes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using Adform.Bloom.Api.Services;
using Adform.Bloom.Api.Swagger.Filters;
using Swashbuckle.AspNetCore.Filters;

namespace Adform.Bloom.Api.Capabilities
{
    public static class StartupSwagger
    {
        private const string MitLicenseUrl = "https://opensource.org/licenses/MIT";

        public static IServiceCollection ConfigureSwagger(this IServiceCollection services,
            IConfigurationRoot configuration)
        {
            return services.AddSwagger(options =>
            {
                options.RegisterDefaultDocs(new OpenApiInfo
                {
                    Title = configuration.GetValue<string>("Swagger:Title"),
                    Description = configuration.GetValue<string>("Swagger:Description"),
                    TermsOfService = new Uri("Shareware", UriKind.Relative),
                    License = new OpenApiLicense {Name = "MIT", Url = new Uri(MitLicenseUrl)},
                    Contact = new OpenApiContact
                    {
                        Name = configuration.GetValue<string>("Swagger:Contact:Name"),
                        Email = configuration.GetValue<string>("Swagger:Contact:Email")
                    }
                });

                options.AddAuth(new OAuth2ClientCredentialsScheme(
                    $"{configuration.GetValue<string>("OAuth:Authority")}/connect/authorize",
                    $"{configuration.GetValue<string>("OAuth:Authority")}/connect/token",
                    new[] {StartupOAuth.Scopes.Full, StartupOAuth.Scopes.Readonly, StartupOAuth.Scopes.FullSubject}));

                options.AddOperationFilter<AddRequestHeaderAcceptFilter>();
                options.AddOperationFilter<AddResponseHeaderWwwAuthenticateFilter>();
                options.AddOperationFilter<AddPaginationResponseHeadersFilter>();
            }).AddSwaggerExamplesFromAssemblyOf<ContentNegotiationFailureResponseExample>();
        }

        public static IApplicationBuilder UseSwaggerUi(this IApplicationBuilder app,
            IApiVersionDescriptionProvider provider,
            IConfigurationRoot configuration)
        {
            return app.UseSwaggerWithUi(options =>
            {
                options.SetDefaults();
                options.RegisterDefaultEndpoints(provider);
                options.AddAuth(new SwaggerOAuth2Options
                {
                    ClientId = configuration.GetValue<string>("OAuth:ClientId"),
                });
            }, setup => { setup.SerializeAsV2 = true; });
        }
    }
}