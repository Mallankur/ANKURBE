using Adform.Bloom.Application.Queries;

namespace Adform.Bloom.Application.Exceptions
{
    public static class ErrorMessages
    {
        public static readonly string LegacyIdsAndTenantIdsCannotBeSet = $"The {nameof(SubjectRuntimeQuery.TenantLegacyIds)} cannot be set with {nameof(SubjectRuntimeQuery.TenantIds)}";

        public static readonly string LegacyIdsMissingTenantType = $"The {nameof(SubjectRuntimeQuery.TenantLegacyIds)} cannot be set without {nameof(SubjectRuntimeQuery.TenantType)}.";
    }
}