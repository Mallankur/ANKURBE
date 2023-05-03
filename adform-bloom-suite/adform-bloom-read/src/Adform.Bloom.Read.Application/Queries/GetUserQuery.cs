using System;
using Adform.Bloom.Read.Domain.Entities;
using MediatR;

namespace Adform.Bloom.Read.Application.Queries;

public class GetUserQuery : IRequest<User?>
{
    public Guid Id { get; }

    public GetUserQuery(Guid id)
    {
        Id = id;
    }
}