using System;
using Adform.Bloom.Read.Domain.Entities;
using MediatR;

namespace Adform.Bloom.Read.Application.Queries;

public class GetBusinessAccountQuery : IRequest<BusinessAccount?>
{
    public Guid Id { get; }

    public GetBusinessAccountQuery(Guid id)
    {
        Id = id;
    }
}