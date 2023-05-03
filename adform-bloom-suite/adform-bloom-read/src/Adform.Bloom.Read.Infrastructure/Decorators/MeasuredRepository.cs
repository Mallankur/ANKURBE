using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Abstractions.Persistence;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.Monitoring.Abstractions.Extensions;

namespace Adform.Bloom.Read.Infrastructure.Decorators;

public class MeasuredRepository<TEntity, UEntityCount> : IRepository<TEntity, UEntityCount>
    where TEntity : BaseEntity
{
    protected static readonly string Type = typeof(TEntity).Name;
    protected readonly ICustomHistogram Histogram;
    private readonly IRepository<TEntity, UEntityCount> _inner;

    public MeasuredRepository(IRepository<TEntity, UEntityCount> inner, ICustomHistogram histogram)
    {
        _inner = inner;
        Histogram = histogram;
    }

    public IDbConnection Connection => _inner.Connection;

    public Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Histogram.Measure(() => _inner.FindByIdAsync(id, cancellationToken), Type, nameof(FindByIdAsync));

    public Task<IEnumerable<UEntityCount>> FindAsync(int offset,
        int limit,
        string orderBy,
        SortingOrder sortingOrder,
        string? search,
        IEnumerable<Guid>? ids,
        int? type, CancellationToken cancellationToken) =>
        Histogram.Measure(
            () => _inner.FindAsync(offset, limit, orderBy, sortingOrder, search, ids, type, cancellationToken),
            Type,
            nameof(FindAsync));
}