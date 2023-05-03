using System;
using System.Collections.Generic;
using Adform.Bloom.Read.Domain.Entities;
using MediatR;

namespace Adform.Bloom.Read.Application.Queries;

public class SearchBusinessAccountQuery : IRequest<IEnumerable<BusinessAccountWithCount>>
{
    public SearchBusinessAccountQuery(int offset, int limit, string orderBy, SortingOrder sortingOrder, string? search, IEnumerable<Guid>? ids, int? type)
    {
        Offset = offset;
        Limit = limit;
        OrderBy = orderBy;
        SortingOrder = sortingOrder;
        Search = search;
        Ids = ids;
        Type = type;
    }

    public int Offset { get; }
    public int Limit { get; }
    public string OrderBy { get; }
    public SortingOrder SortingOrder { get; }
    public string? Search { get; }
    public IEnumerable<Guid>? Ids { get; }
    public int? Type { get; }
}