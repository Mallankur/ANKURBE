using Adform.Bloom.Application.Exceptions;
using Adform.Bloom.Application.Queries;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;

namespace Adform.Bloom.Application.Validators
{
    public class ValidateQuery : IValidateQuery
    {
        public void Validate(SubjectRuntimeQuery query)
        {
            if (!query.TenantLegacyIds.Any()) return;
            if (query.TenantIds.Any())
            {
                throw new BadRequestException(ErrorReasons.ConstraintsViolationReason, ErrorMessages.LegacyIdsAndTenantIdsCannotBeSet);
            }

            if (string.IsNullOrEmpty(query.TenantType))
            {
                throw new BadRequestException(ErrorReasons.ConstraintsViolationReason, ErrorMessages.LegacyIdsMissingTenantType);
            }
        }
    }
}