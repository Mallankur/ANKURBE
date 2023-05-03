using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Adform.Bloom.Runtime.Host.Capabilities
{
    public static class StartupMvc
    {
        public static IMvcBuilder ConfigureMvc(this IServiceCollection services)
        {
            return services
                .AddApiVersioning()
                .AddControllers()
                .ConfigureJson();
        }

        private static IMvcBuilder ConfigureJson(this IMvcBuilder builder)
        {
            builder.AddNewtonsoftJson(f =>
            {
                f.SerializerSettings.Formatting = Formatting.Indented;
                f.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                f.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                f.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                f.SerializerSettings.Converters.Add(new StringEnumConverter());
                f.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            return builder;
        }
    }
}