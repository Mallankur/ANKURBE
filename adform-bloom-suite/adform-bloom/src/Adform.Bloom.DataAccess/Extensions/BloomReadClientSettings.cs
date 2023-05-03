using Adform.Ciam.TokenProvider.Configuration;

namespace Adform.Bloom.DataAccess.Extensions
{
    public class BloomReadClientSettings : OAuthClientConfiguration
    {
        public string GrpcPort { get; set; } = "9696";
    }
}