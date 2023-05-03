using Adform.Bloom.Read.Application.Queries;
using FluentValidation;

namespace Adform.Bloom.Read.Application.Validators;

public class GetUserQueryValidator: AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(c => c.Id).NotNull().NotEmpty();
    }
}