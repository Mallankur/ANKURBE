using System;

namespace Adform.Bloom.Domain.ValueObjects
{
    [Flags]
    public enum ErrorCodes
    {
        TenantDoesNotExist = 0x1,
        SubjectDoesNotExist = 0x2,
        PermissionDoesNotExist = 0x4,
        RoleDoesNotExist = 0x8,

        PolicyDoesNotExist = 0x10,
        FeaturesDoNotExist = 0x20,
        LicensedFeaturesDoNotExist = 0x40,
        SubjectCannotAccessSubject = 0x80,
        SubjectCannotAccessTenant = 0x100,

        SubjectCannotAccessPermission = 0x200,
        SubjectCannotAccessRole = 0x400,
        SubjectCannotAccessFeatures = 0x800,
        SubjectCannotModifyAssignmentsForHimself = 0x1000,

        FeatureDependencyMissing = 0x2000,
        CircularDependency = 0x4000,
        TenantFeatureCoDependenciesConflict = 0x8000,
        SubjectCannotExceedRoleAssignmentLimit = 0X10000,
    }
}
