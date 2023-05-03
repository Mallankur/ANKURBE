using System;
using System.Linq;
using Adform.Bloom.Read.Application.Queries;
using FluentValidation;

namespace Adform.Bloom.Read.Application.Validators;

public class SearchBusinessAccountQueryValidator: AbstractValidator<SearchBusinessAccountQuery >
{
    public SearchBusinessAccountQueryValidator()
    {
        RuleFor(p => p.Offset).GreaterThan(0);
        RuleFor(p => p.Limit).LessThan(100);
        RuleFor(c => c).Must(p => p.Search is not null && p.Ids is not null);
    }
}