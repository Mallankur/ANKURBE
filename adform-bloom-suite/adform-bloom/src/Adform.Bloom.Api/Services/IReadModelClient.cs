using System.Threading.Tasks;

namespace Adform.Bloom.Api.Services
{
    public interface IReadModelClient
    {
        Task<bool> IsHealthy();
    }
}