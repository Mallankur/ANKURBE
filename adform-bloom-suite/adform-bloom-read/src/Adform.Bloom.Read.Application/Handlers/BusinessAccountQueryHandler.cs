using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Abstractions.Persistence;
using Adform.Bloom.Read.Application.Queries;
using Adform.Bloom.Read.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Adform.Bloom.Read.Application.Handlers;

public class BusinessAccountQueryHandler :
    IRequestHandler<GetBusinessAccountQuery, BusinessAccount>,
    IRequestHandler<SearchBusinessAccountQuery, IEnumerable<BusinessAccountWithCount>>
{
    private readonly IRepository<BusinessAccount, BusinessAccountWithCount> _businessAccountRepository;
    private readonly ILogger<BusinessAccountQueryHandler> _logger;

    public BusinessAccountQueryHandler(IRepository<BusinessAccount, BusinessAccountWithCount> businessAccountRepository,
        ILogger<BusinessAccountQueryHandler> logger)
    {
        _businessAccountRepository = businessAccountRepository;
        _logger = logger;
    }

    public Task<BusinessAccount?> Handle(GetBusinessAccountQuery request, CancellationToken cancellationToken)
        => _businessAccountRepository.FindByIdAsync(request.Id, cancellationToken);

    public Task<IEnumerable<BusinessAccountWithCount>> Handle(SearchBusinessAccountQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Handling {typeof(BusinessAccountQueryHandler).Name}");

        if (request.Search is null && request.Ids is null)
        {
            throw new Exception("Reading all records is not supported.");
        }

        return _businessAccountRepository.FindAsync(
            request.Offset,
            request.Limit,
            request.OrderBy,
            request.SortingOrder,
            request.Search,
            request.Ids,
            request.Type, cancellationToken);
    }
}