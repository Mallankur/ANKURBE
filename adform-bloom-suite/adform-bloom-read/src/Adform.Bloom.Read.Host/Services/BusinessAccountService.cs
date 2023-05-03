using System;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Queries;
using Adform.Bloom.Read.Contracts;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Read.Domain.Entities;
using MediatR;
using ProtoBuf.Grpc;
using SortingOrder = Adform.Bloom.Read.Domain.Entities.SortingOrder;

namespace Adform.Bloom.Read.Host.Services;

public class BusinessAccountService : IBusinessAccountService
{
    private readonly IMediator _mediator;

    public BusinessAccountService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<BusinessAccountGetResult> GetBusinessAccount(GetRequest request, CallContext context)
    {
        var ba = await _mediator.Send(new GetBusinessAccountQuery(request.Id), context.CancellationToken);
        return new BusinessAccountGetResult
        {
            BusinessAccount = ba is null ? null : MapBusinessAccount(ba)
        };
    }

    public async Task<BusinessAccountSearchResult> FindBusinessAccounts(BusinessAccountSearchRequest request,
        CallContext context)
    {
        var result = await _mediator.Send(
            new SearchBusinessAccountQuery(request.Offset, 
                request.Limit, 
                request.OrderBy, 
                (SortingOrder)request.SortingOrder, 
                request.Search, 
                request.Ids, 
                (int?)request.Type), context.CancellationToken);
        var businessAccounts = result.Where(o => o.Id != Guid.Empty).Select(MapBusinessAccount).ToList();
        return new BusinessAccountSearchResult
        {
            Offset = request.Offset,
            Limit = request.Limit,
            TotalItems = result.FirstOrDefault()?.TotalCount ?? 0,
            BusinessAccounts = businessAccounts
        };
    }

    private static BusinessAccountResult MapBusinessAccount(BusinessAccount ba) => new BusinessAccountResult
    {
        Id = ba.Id,
        LegacyId = ba.LegacyId,
        Name = ba.Name ?? string.Empty,
        Status = (BusinessAccountStatus)ba.Status,
        Type = (BusinessAccountType)ba.Type
    };
}