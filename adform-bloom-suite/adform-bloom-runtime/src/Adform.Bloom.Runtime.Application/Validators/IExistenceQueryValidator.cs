using Adform.Bloom.Application.Queries;
using FluentValidation.Results;

namespace Adform.Bloom.Application.Validators
{
    public interface IExistenceQueryValidator
    {
        ValidationResult Validate(NodeExistenceQuery query);
        ValidationResult Validate(RoleExistenceQuery query);
        ValidationResult Validate(LegacyTenantExistenceQuery query);
        ValidationResult Validate(SubjectIntersectionQuery query);
    }
}