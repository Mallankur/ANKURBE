namespace Adform.Bloom.Application.Abstractions.Persistence
{
    public interface IAdformTenantProvider
    {
        Task<Guid> GetAdformTenant(Guid SubjectId, CancellationToken cancellationToken = default);
    }
}