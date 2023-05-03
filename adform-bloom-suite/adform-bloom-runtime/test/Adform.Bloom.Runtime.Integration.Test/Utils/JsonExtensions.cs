using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Adform.Bloom.Runtime.Integration.Test.Utils
{
    public static class JsonExtensions
    {
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var json = await content.ReadAsStringAsync();
            var value = JsonSerializer.Deserialize<T>(json, options);
            return value;
        }

        public static T ToObject<T>(this JsonElement element)
        {
            var json = element.GetRawText();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}