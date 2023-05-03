using System;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Queries;
using Adform.Bloom.Read.Contracts.User;
using Adform.Bloom.Read.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using ProtoBuf.Grpc;
using SortingOrder = Adform.Bloom.Read.Domain.Entities.SortingOrder;

namespace Adform.Bloom.Read.Host.Services;

public class UserService : IUserService
{
    private readonly IMediator _mediator;

    public UserService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<UserSearchResult> Find(UserSearchRequest request, CallContext context)
    {
        var result = await _mediator.Send(
            new SearchUserQuery(request.Offset,
                request.Limit,
                request.OrderBy,
                (SortingOrder) request.SortingOrder,
                request.Search,
                request.Ids,
                (int?) request.Type), context.CancellationToken);
        var users = result.Where(o => o.Id != Guid.Empty).Select(MapUser).ToList();
        return new UserSearchResult
        {
            Offset = request.Offset,
            Limit = request.Limit,
            TotalItems = result.FirstOrDefault()?.TotalCount ?? 0,
            Users = users
        };
    }

    public async Task<UserGetResult> Get(UserGetRequest request, CallContext context)
    {
        var user = await _mediator.Send(new GetUserQuery(request.Id), context.CancellationToken);
        return new UserGetResult
        {
            User = user is null ? null : MapUser(user)
        };
    }

    private static UserResult MapUser(User user)
    {
        return new UserResult
        {
            Id = user.Id,
            Email = user.Email,
            Locale = user.Locale,
            Name = user.Name,
            Phone = user.Phone,
            Timezone = user.Timezone,
            Username = user.Username,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Company = user.Company ?? string.Empty,
            Title = user.Title,
            SecurityNotifications = user.SecurityNotifications.GetValueOrDefault(),
            TwoFaEnabled = user.TwoFaEnabled.GetValueOrDefault(),
            Status = (UserStatus) user.Status.GetValueOrDefault(),
            Type = (UserType) user.Type
        };
    }
}