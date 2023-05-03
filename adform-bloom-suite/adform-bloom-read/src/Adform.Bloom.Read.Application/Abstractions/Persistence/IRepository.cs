using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Read.Domain.Entities;

namespace Adform.Bloom.Read.Application.Abstractions.Persistence;

public interface IRepository<T, U> where T : BaseEntity
{
    IDbConnection Connection { get; }
    Task<T?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<U>> FindAsync(
        int offset,
        int limit,
        string orderBy,
        SortingOrder sortingOrder,
        string? search,
        IEnumerable<Guid>? ids,
        int? type,
        CancellationToken cancellationToken);
}