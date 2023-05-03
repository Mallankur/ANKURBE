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

public class UserQueryHandler :
    IRequestHandler<SearchUserQuery, IEnumerable<UserWithCount>>,
    IRequestHandler<GetUserQuery, User?>
{
    private readonly IRepository<User, UserWithCount> _userRepository;
    private readonly ILogger<UserQueryHandler> _logger;

    public UserQueryHandler(
        IRepository<User, UserWithCount> userRepository, ILogger<UserQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public Task<IEnumerable<UserWithCount>> Handle(SearchUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Handling {typeof(UserQueryHandler).Name}");

        if (request.Search is null && request.Ids is null)
        {
            throw new Exception("Reading all records is not supported.");
        }

        return _userRepository.FindAsync(
            request.Offset,
            request.Limit,
            request.OrderBy,
            request.SortingOrder,
            request.Search,
            request.Ids,
            request.Type,
            cancellationToken);
    }

    public Task<User?> Handle(GetUserQuery request, CancellationToken cancellationToken)
        => _userRepository.FindByIdAsync(request.Id, cancellationToken);

}