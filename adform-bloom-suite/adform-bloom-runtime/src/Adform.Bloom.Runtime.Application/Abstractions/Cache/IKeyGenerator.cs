namespace Adform.Bloom.Application.Abstractions.Cache
{
    public interface IKeyGenerator<T>
    {
        string GenerateKey(T query);
    }
}