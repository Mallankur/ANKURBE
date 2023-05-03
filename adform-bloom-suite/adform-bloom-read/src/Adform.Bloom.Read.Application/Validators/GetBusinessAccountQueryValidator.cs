using Adform.Bloom.Read.Application.Queries;
using FluentValidation;

namespace Adform.Bloom.Read.Application.Validators;

public class GetBusinessAccountQueryValidator: AbstractValidator<GetBusinessAccountQuery>
{
    public GetBusinessAccountQueryValidator()
    {
        RuleFor(c => c.Id).NotNull().NotEmpty();
    }
}