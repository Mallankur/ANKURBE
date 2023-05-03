using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Adform.Bloom.Seeder
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var result=new PermissionGraph(configuration);
            await result.GeneratePermission("IAM Management", "CIAM", "Subject");

            //await Graph.CreateGraph();
        }
    }
}