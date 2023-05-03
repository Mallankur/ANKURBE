using FluentResults;
using MediatR;

namespace Adform.Bloom.Application.Abstractions.Persistence
{
    public interface IExistenceProvider
    {
        Task<Result<bool>> CheckExistence(IRequest<Result<bool>> existenceRequest, CancellationToken cancellationToken = default);
    }
}