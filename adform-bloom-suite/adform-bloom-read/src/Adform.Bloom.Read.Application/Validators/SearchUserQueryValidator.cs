using Adform.Bloom.Read.Application.Queries;
using FluentValidation;

namespace Adform.Bloom.Read.Application.Validators;

public class SearchUserQueryValidator: AbstractValidator<SearchUserQuery >
{
    public SearchUserQueryValidator()
    {
        RuleFor(p => p.Offset).GreaterThan(0);
        RuleFor(p => p.Limit).LessThan(100);
        RuleFor(c => c).Must(p => p.Search is not null && p.Ids is not null);
    }
}