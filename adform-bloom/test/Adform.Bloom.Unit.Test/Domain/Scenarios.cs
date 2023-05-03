using System;
using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Domain.ValueObjects;
using Xunit;

namespace Adform.Bloom.Unit.Test.Domain
{
    public static class Scenarios
    {
        public static TheoryData<CanAssignPermissionToRoleScenario> GenerateCanAssignPermissionToRoleScenarios()
        {
            var data = new TheoryData<CanAssignPermissionToRoleScenario>
            {
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = false,
                    DoesRoleExist = true,
                    HasVisibilityToPermissionAsync = true,
                    HasVisibilityToRoleAsync = true,
                    Callback = r => r.Error == ErrorCodes.PermissionDoesNotExist
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = true,
                    DoesRoleExist = false,
                    HasVisibilityToPermissionAsync = true,
                    HasVisibilityToRoleAsync = true,
                    Callback = r => r.Error == ErrorCodes.RoleDoesNotExist
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = true,
                    DoesRoleExist = true,
                    HasVisibilityToPermissionAsync = false,
                    HasVisibilityToRoleAsync = true,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessPermission
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = true,
                    DoesRoleExist = true,
                    HasVisibilityToPermissionAsync = true,
                    HasVisibilityToRoleAsync = false,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessRole
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = false,
                    DoesRoleExist = false,
                    HasVisibilityToPermissionAsync = true,
                    HasVisibilityToRoleAsync = true,
                    Callback = r => r.Error == (ErrorCodes.PermissionDoesNotExist | ErrorCodes.RoleDoesNotExist)
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = true,
                    DoesRoleExist = false,
                    HasVisibilityToPermissionAsync = false,
                    HasVisibilityToRoleAsync = true,
                    Callback = r => r.Error == (ErrorCodes.RoleDoesNotExist | ErrorCodes.SubjectCannotAccessPermission)
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = true,
                    DoesRoleExist = true,
                    HasVisibilityToPermissionAsync = false,
                    HasVisibilityToRoleAsync = false,
                    Callback = r =>
                        r.Error == (ErrorCodes.SubjectCannotAccessPermission | ErrorCodes.SubjectCannotAccessRole)
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = false,
                    DoesRoleExist = true,
                    HasVisibilityToPermissionAsync = true,
                    HasVisibilityToRoleAsync = false,
                    Callback = r =>
                        r.Error == (ErrorCodes.PermissionDoesNotExist | ErrorCodes.SubjectCannotAccessRole)
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = false,
                    DoesRoleExist = false,
                    HasVisibilityToPermissionAsync = false,
                    HasVisibilityToRoleAsync = true,
                    Callback = r => r.Error == (ErrorCodes.PermissionDoesNotExist | ErrorCodes.RoleDoesNotExist | ErrorCodes.SubjectCannotAccessPermission)
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = true,
                    DoesRoleExist = false,
                    HasVisibilityToPermissionAsync = false,
                    HasVisibilityToRoleAsync = false,
                    Callback = r => r.Error == (ErrorCodes.RoleDoesNotExist | ErrorCodes.SubjectCannotAccessPermission | ErrorCodes.SubjectCannotAccessRole)
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = false,
                    DoesRoleExist = true,
                    HasVisibilityToPermissionAsync = false,
                    HasVisibilityToRoleAsync = false,
                    Callback = r => r.Error == (ErrorCodes.PermissionDoesNotExist | ErrorCodes.SubjectCannotAccessPermission | ErrorCodes.SubjectCannotAccessRole)
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = false,
                    DoesRoleExist = false,
                    HasVisibilityToPermissionAsync = true,
                    HasVisibilityToRoleAsync = false,
                    Callback = r => r.Error == (ErrorCodes.PermissionDoesNotExist | ErrorCodes.RoleDoesNotExist | ErrorCodes.SubjectCannotAccessRole)
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = false,
                    DoesRoleExist = false,
                    HasVisibilityToPermissionAsync = false,
                    HasVisibilityToRoleAsync = false,
                    Callback = r => r.Error == (ErrorCodes.PermissionDoesNotExist | ErrorCodes.RoleDoesNotExist |
                                                ErrorCodes.SubjectCannotAccessPermission |
                                                ErrorCodes.SubjectCannotAccessRole)
                },
                new CanAssignPermissionToRoleScenario
                {
                    DoesPermissionExist = true,
                    DoesRoleExist = true,
                    HasVisibilityToPermissionAsync = true,
                    HasVisibilityToRoleAsync = true,
                    Callback = r => r.Error == 0
                }
            };
            return data;
        }

        public static TheoryData<CanCreateRoleScenario> GenerateCanCreateRoleScenario()
        {
            var data = new TheoryData<CanCreateRoleScenario>
            {
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = true,
                    DoesTenantExist = true,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = false,
                    PrincipalTenantId = null,
                    Callback = r => r.Error == 0
                },
                new CanCreateRoleScenario
                {
                    FeatureIds = null,
                    DoesPolicyExist = true,
                    DoesTenantExist = true,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = false,
                    PrincipalTenantId = null,
                    Callback = r => r.Error == 0
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = false,
                    DoesTenantExist = true,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = false,
                    PrincipalTenantId = null,
                    Callback = r => r.Error == ErrorCodes.PolicyDoesNotExist
                },
                new CanCreateRoleScenario
                {
                    PolicyId = null,
                    DoesPolicyExist = true,
                    DoesTenantExist = true,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = false,
                    PrincipalTenantId = null,
                    Callback = r => r.Error == ErrorCodes.PolicyDoesNotExist
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = true,
                    DoesTenantExist = false,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = false,
                    PrincipalTenantId = null,
                    Callback = r => r.Error == ErrorCodes.TenantDoesNotExist
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = true,
                    DoesTenantExist = true,
                    DoFeaturesExist = false,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = false,
                    PrincipalTenantId = null,
                    Callback = r => r.Error == ErrorCodes.FeaturesDoNotExist
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = false,
                    DoesTenantExist = false,
                    DoFeaturesExist = false,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = false,
                    PrincipalTenantId = null,
                    Callback = r =>
                        r.Error == (ErrorCodes.PolicyDoesNotExist | ErrorCodes.TenantDoesNotExist |
                                    ErrorCodes.FeaturesDoNotExist)
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = true,
                    DoesTenantExist = true,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = false,
                    IsTemplateRole = false,
                    PrincipalTenantId = null,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessFeatures
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = true,
                    DoesTenantExist = true,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = false,
                    PrincipalTenantId = Guid.NewGuid().ToString(),
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessTenant
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = false,
                    DoesTenantExist = false,
                    DoFeaturesExist = false,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = false,
                    IsTemplateRole = false,
                    PrincipalTenantId = Guid.NewGuid().ToString(),
                    Callback = r =>
                        r.Error == (ErrorCodes.PolicyDoesNotExist | ErrorCodes.TenantDoesNotExist |
                                    ErrorCodes.FeaturesDoNotExist
                                    | ErrorCodes.SubjectCannotAccessTenant | ErrorCodes.SubjectCannotAccessFeatures)
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = false,
                    DoesTenantExist = false,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = false,
                    HasVisibilityToFeature = false,
                    IsTemplateRole = false,
                    PrincipalTenantId = Guid.NewGuid().ToString(),
                    Callback = r =>
                        r.Error == (ErrorCodes.PolicyDoesNotExist | ErrorCodes.TenantDoesNotExist |
                                    ErrorCodes.FeatureDependencyMissing
                                    | ErrorCodes.SubjectCannotAccessTenant | ErrorCodes.SubjectCannotAccessFeatures)
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = true,
                    DoesTenantExist = true,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = true,
                    PrincipalTenantId = Guid.NewGuid().ToString(),
                    Callback = r =>
                        r.Error == (ErrorCodes.SubjectCannotAccessTenant | ErrorCodes.SubjectCannotAccessRole)
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = true,
                    DoesTenantExist = true,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = true,
                    PrincipalTenantId = null,
                    Callback = r =>
                        r.Error == ErrorCodes.SubjectCannotAccessRole
                },
                new CanCreateRoleScenario
                {
                    DoesPolicyExist = true,
                    DoesTenantExist = true,
                    DoFeaturesExist = true,
                    IsFeatureDependencySatisfied = true,
                    HasVisibilityToFeature = true,
                    IsTemplateRole = true,
                    RoleName = ClaimPrincipalExtensions.AdformAdmin,
                    PrincipalTenantId = null,
                    Callback = r => r.Error == 0
                }
            };
            return data;
        }

        public static TheoryData<CanUpdateRoleScenario> GenerateCanUpdateRoleScenario()
        {
            var data = new TheoryData<CanUpdateRoleScenario>
            {
                new CanUpdateRoleScenario
                {
                    HasVisibilityToRoleAsync = true,
                    DoesRoleExist = true,
                    Callback = r => r.Error == 0
                },
                new CanUpdateRoleScenario
                {
                    HasVisibilityToRoleAsync = false,
                    DoesRoleExist = true,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessRole
                },
                new CanUpdateRoleScenario
                {
                    HasVisibilityToRoleAsync = true,
                    DoesRoleExist = false,
                    Callback = r => r.Error == ErrorCodes.RoleDoesNotExist
                },
                new CanUpdateRoleScenario
                {
                    HasVisibilityToRoleAsync = false,
                    DoesRoleExist = false,
                    Callback = r => r.Error == (ErrorCodes.RoleDoesNotExist | ErrorCodes.SubjectCannotAccessRole)
                },
                new CanUpdateRoleScenario
                {
                    HasVisibilityToRoleAsync = false,
                    DoesRoleExist = false,
                    Callback = r => r.Error == (ErrorCodes.RoleDoesNotExist | ErrorCodes.SubjectCannotAccessRole)
                }
            };
            return data;
        }

        public static TheoryData<CanCreateSubjectScenario> GenerateCanCreateSubjectScenario()
        {
            var data = new TheoryData<CanCreateSubjectScenario>()
            {
                new CanCreateSubjectScenario
                {
                    DoesSubjectExist = true,
                    Callback = false
                },
                new CanCreateSubjectScenario
                {
                    DoesSubjectExist = false,
                    Callback = true
                }
            };
            return data;
        }

        public static TheoryData<CanAssignSubjectToRolesScenario> GenerateCanAssignSubjectToRolesScenario()
        {
            var data = new TheoryData<CanAssignSubjectToRolesScenario>
            {
                new CanAssignSubjectToRolesScenario
                {
                    IsSameSubject = true,
                    DoSubjectExist = false,
                    DoRolesExist = false,
                    DoTenantsExist = false,
                    HasVisibilityToRolesAsync = false,
                    HasEnoughRoleAssignmentCapacity = false,
                    Callback = r => r.Error == (ErrorCodes.TenantDoesNotExist | ErrorCodes.SubjectDoesNotExist | ErrorCodes.RoleDoesNotExist | ErrorCodes.SubjectCannotAccessRole |
                                                ErrorCodes.SubjectCannotModifyAssignmentsForHimself | ErrorCodes.SubjectCannotExceedRoleAssignmentLimit)
                },
                new CanAssignSubjectToRolesScenario
                {
                    IsSameSubject = false,
                    DoSubjectExist = true,
                    DoRolesExist = true,
                    DoTenantsExist = true,
                    HasVisibilityToRolesAsync = true,
                    HasEnoughRoleAssignmentCapacity = true,
                    Assignments = new[] {new RoleTenant{TenantId = Guid.NewGuid(), RoleId = Guid.NewGuid()}},
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessTenant
                },
                new CanAssignSubjectToRolesScenario
                {
                    IsSameSubject = false,
                    DoSubjectExist = true,
                    DoRolesExist = true,
                    DoTenantsExist = false,
                    HasVisibilityToRolesAsync = false,
                    HasEnoughRoleAssignmentCapacity = true,
                    Callback = r => r.Error == (ErrorCodes.TenantDoesNotExist | ErrorCodes.SubjectCannotAccessRole)
                },
                new CanAssignSubjectToRolesScenario
                {
                    IsSameSubject = false,
                    DoSubjectExist = true,
                    DoRolesExist = true,
                    DoTenantsExist = true,
                    HasVisibilityToRolesAsync = false,
                    HasEnoughRoleAssignmentCapacity = true,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessRole
                },
                new CanAssignSubjectToRolesScenario
                {
                    IsSameSubject = false,
                    DoSubjectExist = false,
                    DoRolesExist = true,
                    DoTenantsExist = true,
                    HasVisibilityToRolesAsync = true,
                    HasEnoughRoleAssignmentCapacity = true,
                    Callback = r => r.Error == ErrorCodes.SubjectDoesNotExist
                },
                new CanAssignSubjectToRolesScenario
                {
                    IsSameSubject = false,
                    DoSubjectExist = true,
                    DoRolesExist = true,
                    DoTenantsExist = true,
                    HasVisibilityToRolesAsync = true,
                    HasEnoughRoleAssignmentCapacity = false,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotExceedRoleAssignmentLimit
                },
                new CanAssignSubjectToRolesScenario
                {
                    IsSameSubject = true,
                    DoSubjectExist = true,
                    DoRolesExist = true,
                    DoTenantsExist = true,
                    HasVisibilityToRolesAsync = true,
                    HasEnoughRoleAssignmentCapacity = true,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotModifyAssignmentsForHimself
                }
            };
            return data;
        }

        public static TheoryData<CanAssignRoleToFeaturesScenario> GenerateCanAssignRoleToFeaturesScenario()
        {
            var data = new TheoryData<CanAssignRoleToFeaturesScenario>
            {
                new CanAssignRoleToFeaturesScenario
                {
                    HasVisibilityToRole = true,
                    DoesRoleExist = true,
                    HasVisibilityToFeatures = true,
                    Callback = r => r.Error == 0
                },
                new CanAssignRoleToFeaturesScenario
                {
                    HasVisibilityToRole = false,
                    DoesRoleExist = true,
                    HasVisibilityToFeatures = true,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessRole
                },
                new CanAssignRoleToFeaturesScenario
                {
                    HasVisibilityToRole = true,
                    DoesRoleExist = false,
                    HasVisibilityToFeatures = true,
                    Callback = r => r.Error == ErrorCodes.RoleDoesNotExist
                },
                new CanAssignRoleToFeaturesScenario
                {
                    HasVisibilityToRole = true,
                    DoesRoleExist = true,
                    HasVisibilityToFeatures = false,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessFeatures
                },
                new CanAssignRoleToFeaturesScenario
                {
                    HasVisibilityToRole = false,
                    DoesRoleExist = false,
                    HasVisibilityToFeatures = true,
                    Callback = r => r.Error == (ErrorCodes.SubjectCannotAccessRole | ErrorCodes.RoleDoesNotExist)
                },
                new CanAssignRoleToFeaturesScenario
                {
                    HasVisibilityToRole = true,
                    DoesRoleExist = false,
                    HasVisibilityToFeatures = false,
                    Callback = r => r.Error == (ErrorCodes.RoleDoesNotExist | ErrorCodes.SubjectCannotAccessFeatures)
                },
                new CanAssignRoleToFeaturesScenario
                {
                    HasVisibilityToRole = false,
                    DoesRoleExist = true,
                    HasVisibilityToFeatures = false,
                    Callback = r => r.Error == (ErrorCodes.SubjectCannotAccessRole | ErrorCodes.SubjectCannotAccessFeatures)
                },
                new CanAssignRoleToFeaturesScenario
                {
                    HasVisibilityToRole = false,
                    DoesRoleExist = false,
                    HasVisibilityToFeatures = false,
                    Callback = r => r.Error == (ErrorCodes.RoleDoesNotExist | ErrorCodes.SubjectCannotAccessRole | ErrorCodes.SubjectCannotAccessFeatures)
                }
            };
            return data;
        }

        public static TheoryData<CanCreateFeatureCoDepenencyScenario> GenerateCanCreateFeatureCoDepenencyScenario()
        {
            var data = new TheoryData<CanCreateFeatureCoDepenencyScenario>
            {
                new CanCreateFeatureCoDepenencyScenario
                {
                    DoesFeatureExist = true,
                    DoesDependsOnExist = true,
                    HasVisibilityToFeature = true,
                    HasVisibilityToDependentOn = true,
                    IsAssignment = true,
                    IsCircuralDependency = false,
                    Callback = r => r.Error == 0
                },
                new CanCreateFeatureCoDepenencyScenario
                {
                    DoesFeatureExist = true,
                    DoesDependsOnExist = true,
                    HasVisibilityToFeature = true,
                    HasVisibilityToDependentOn = true,
                    IsAssignment = false,
                    IsCircuralDependency = true,
                    Callback = r => r.Error == 0
                },
                new CanCreateFeatureCoDepenencyScenario
                {
                    DoesFeatureExist = true,
                    DoesDependsOnExist = true,
                    HasVisibilityToFeature = false,
                    HasVisibilityToDependentOn = true,
                    IsAssignment = true,
                    IsCircuralDependency = false,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessFeatures
                },
                new CanCreateFeatureCoDepenencyScenario
                {
                    DoesFeatureExist = true,
                    DoesDependsOnExist = true,
                    HasVisibilityToFeature = true,
                    HasVisibilityToDependentOn = false,
                    IsAssignment = true,
                    IsCircuralDependency = false,
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessFeatures
                },
                new CanCreateFeatureCoDepenencyScenario
                {
                    DoesFeatureExist = true,
                    DoesDependsOnExist = true,
                    HasVisibilityToFeature = true,
                    HasVisibilityToDependentOn = true,
                    IsAssignment = true,
                    IsCircuralDependency = true,
                    Callback = r => r.Error == ErrorCodes.CircularDependency
                },
                new CanCreateFeatureCoDepenencyScenario
                {
                    DoesFeatureExist = true,
                    DoesDependsOnExist = false,
                    HasVisibilityToFeature = true,
                    HasVisibilityToDependentOn = true,
                    IsAssignment = true,
                    IsCircuralDependency = true,
                    Callback = r => r.Error == (ErrorCodes.CircularDependency | ErrorCodes.FeaturesDoNotExist)
                },
                new CanCreateFeatureCoDepenencyScenario
                {
                    DoesFeatureExist = true,
                    DoesDependsOnExist = true,
                    HasVisibilityToFeature = false,
                    HasVisibilityToDependentOn = true,
                    IsAssignment = true,
                    IsCircuralDependency = true,
                    Callback = r => r.Error == (ErrorCodes.SubjectCannotAccessFeatures | ErrorCodes.CircularDependency)
                },
                new CanCreateFeatureCoDepenencyScenario
                {
                    DoesFeatureExist = false,
                    DoesDependsOnExist = true,
                    HasVisibilityToFeature = false,
                    HasVisibilityToDependentOn = true,
                    IsAssignment = true,
                    IsCircuralDependency = true,
                    Callback = r => r.Error == (ErrorCodes.SubjectCannotAccessFeatures | ErrorCodes.CircularDependency |
                                                ErrorCodes.FeaturesDoNotExist)
                },
            };
            return data;
        }

        public static TheoryData<FilterFeatureIdsWithAccessDeniedScenario> GenerateFilterFeatureIdsWithAccessDeniedScenario()
        {
            var data = new TheoryData<FilterFeatureIdsWithAccessDeniedScenario>
            {
                new FilterFeatureIdsWithAccessDeniedScenario
                {
                    InitialFeatureList = new List<Guid>(){new Guid("b6d03b1e-d06f-43d1-8795-dae81e0e2e3f"), new Guid("696cc2d4-1005-455e-a557-ac2bcd72e6f4") },
                    DeniedFeatureList = new List<Guid>(){ new Guid("b6d03b1e-d06f-43d1-8795-dae81e0e2e3f") },
                    Callback = r => r.Count == 1 && r.First() == new Guid("b6d03b1e-d06f-43d1-8795-dae81e0e2e3f")
                },
                new FilterFeatureIdsWithAccessDeniedScenario
                {
                    InitialFeatureList = new List<Guid>(){new Guid("b6d03b1e-d06f-43d1-8795-dae81e0e2e3f"), new Guid("696cc2d4-1005-455e-a557-ac2bcd72e6f4") },
                    DeniedFeatureList = new List<Guid>(),
                    Callback = r=> r.Count == 0
                },
            };
            return data;
        }

        public static TheoryData<CanUnassignSubjectFromRolesScenario> GenerateCanUnassignSubjectFromRolesScenario()
        {
            var data = new TheoryData<CanUnassignSubjectFromRolesScenario>
            {
                new CanUnassignSubjectFromRolesScenario
                {
                    IsSameSubject = true,
                    DoSubjectExist = false,
                    DoRolesExist = false,
                    DoTenantsExist = false,
                    HasVisibilityToRolesAsync = false,
                    HasVisibilityToSubjectAsync = true,
                    HasEnoughRoleAssignmentCapacity = true,
                    Callback = r => r.Error == (ErrorCodes.SubjectCannotModifyAssignmentsForHimself | ErrorCodes.SubjectDoesNotExist | ErrorCodes.RoleDoesNotExist | ErrorCodes.TenantDoesNotExist |
                                               ErrorCodes.SubjectCannotAccessRole)
                },
                new CanUnassignSubjectFromRolesScenario
                {
                    IsSameSubject = false,
                    DoSubjectExist = true,
                    DoRolesExist = true,
                    DoTenantsExist = true,
                    HasVisibilityToRolesAsync = true,
                    HasVisibilityToSubjectAsync = true,
                    HasEnoughRoleAssignmentCapacity = true,
                    Assignments = new[] {new RoleTenant() {TenantId = Guid.NewGuid(), RoleId=Guid.NewGuid()}},
                    Callback = r => r.Error == ErrorCodes.SubjectCannotAccessTenant
                },
                new CanUnassignSubjectFromRolesScenario
                {
                    IsSameSubject = false,
                    DoSubjectExist = true,
                    DoRolesExist = true,
                    DoTenantsExist = false,
                    HasVisibilityToRolesAsync = false,
                    HasVisibilityToSubjectAsync = true,
                    HasEnoughRoleAssignmentCapacity = true,
                    Callback = r => r.Error == (ErrorCodes.TenantDoesNotExist | ErrorCodes.SubjectCannotAccessRole)
                },
                new CanUnassignSubjectFromRolesScenario
                {
                    IsSameSubject = false,
                    DoSubjectExist = true,
                    DoRolesExist = true,
                    DoTenantsExist = true,
                    HasVisibilityToRolesAsync = false,
                    HasVisibilityToSubjectAsync = true,
                    HasEnoughRoleAssignmentCapacity = false,
                    Callback = r => r.Error == (ErrorCodes.SubjectCannotAccessRole | ErrorCodes.SubjectCannotExceedRoleAssignmentLimit)
                },
                new CanUnassignSubjectFromRolesScenario
                {
                    IsSameSubject = false,
                    DoSubjectExist = false,
                    DoRolesExist = true,
                    DoTenantsExist = true,
                    HasVisibilityToRolesAsync = true,
                    HasVisibilityToSubjectAsync = true,
                    HasEnoughRoleAssignmentCapacity = true,
                    Callback = r => r.Error == ErrorCodes.SubjectDoesNotExist
                },
                new CanUnassignSubjectFromRolesScenario
                {
                    IsSameSubject = true,
                    DoSubjectExist = true,
                    DoRolesExist = true,
                    DoTenantsExist = true,
                    HasVisibilityToRolesAsync = true,
                    HasVisibilityToSubjectAsync = false,
                    HasEnoughRoleAssignmentCapacity = true,
                    Callback = r => r.Error == (ErrorCodes.SubjectCannotAccessSubject | ErrorCodes.SubjectCannotModifyAssignmentsForHimself)
                }
            };
            return data;
        }

        public static TheoryData<CanAssignTenantToFeatureAsyncScenario> GenerateCanAssignTenantToFeatureAsyncScenario()
        {
            var data = new TheoryData<CanAssignTenantToFeatureAsyncScenario>
            {
                new CanAssignTenantToFeatureAsyncScenario
                {
                    DoesFeatureExist = true,
                    DoesTenantExist = true,
                    HasVisibilityToTenant = true,
                    HasTenantFeatureCoDependenciesConflict = false,
                    IsAssignOperation = true,
                    Callback = r => r.Error == 0
                },
                new CanAssignTenantToFeatureAsyncScenario
                {
                    DoesFeatureExist = true,
                    DoesTenantExist = true,
                    HasVisibilityToTenant = true,
                    HasTenantFeatureCoDependenciesConflict = false,
                    IsAssignOperation = false,
                    Callback = r => r.Error == 0
                },
                new CanAssignTenantToFeatureAsyncScenario
                {
                    DoesFeatureExist = false,
                    DoesTenantExist = false,
                    HasVisibilityToTenant = false,
                    HasTenantFeatureCoDependenciesConflict = true,
                    IsAssignOperation = true,
                    Callback = r => r.Error == (ErrorCodes.FeaturesDoNotExist | ErrorCodes.TenantDoesNotExist |
                                                ErrorCodes.SubjectCannotAccessTenant |
                                                ErrorCodes.TenantFeatureCoDependenciesConflict)
                },
                new CanAssignTenantToFeatureAsyncScenario
                {
                    DoesFeatureExist = false,
                    DoesTenantExist = false,
                    HasVisibilityToTenant = false,
                    HasTenantFeatureCoDependenciesConflict = true,
                    IsAssignOperation = false,
                    Callback = r => r.Error == (ErrorCodes.FeaturesDoNotExist | ErrorCodes.TenantDoesNotExist |
                                                ErrorCodes.SubjectCannotAccessTenant |
                                                ErrorCodes.TenantFeatureCoDependenciesConflict)
                }
            };
            return data;
        }


        public static TheoryData<CanAssignLicensedFeatureToTenantAsyncScenario> GenerateCanAssignLicensedFeatureToTenantAsyncScenario()
        {
            var data = new TheoryData<CanAssignLicensedFeatureToTenantAsyncScenario>
            {
                new CanAssignLicensedFeatureToTenantAsyncScenario
                {
                    DoesLicensedFeatureExist = true,
                    DoesTenantExist = true,
                    HasVisibilityToTenant = true,
                    Callback = r => r.Error == 0
                },
                new CanAssignLicensedFeatureToTenantAsyncScenario
                {
                    DoesLicensedFeatureExist = false,
                    DoesTenantExist = false,
                    HasVisibilityToTenant = false,
                    Callback = r => r.Error == (ErrorCodes.LicensedFeaturesDoNotExist | ErrorCodes.TenantDoesNotExist |
                                                ErrorCodes.SubjectCannotAccessTenant)
                },
                new CanAssignLicensedFeatureToTenantAsyncScenario
                {
                    DoesLicensedFeatureExist = false,
                    DoesTenantExist = true,
                    HasVisibilityToTenant = true,
                    Callback = r => r.Error == (ErrorCodes.LicensedFeaturesDoNotExist )
                },
                new CanAssignLicensedFeatureToTenantAsyncScenario
                {
                    DoesLicensedFeatureExist = false,
                    DoesTenantExist = false,
                    HasVisibilityToTenant = true,
                    Callback = r => r.Error == (ErrorCodes.LicensedFeaturesDoNotExist | ErrorCodes.TenantDoesNotExist)
                }
            };
            return data;
        }
        public static TheoryData<CanEditRoleAsyncScenario> GenerateCanEditRoleAsyncScenario()
        {
            var data = new TheoryData<CanEditRoleAsyncScenario>
            {
                    new CanEditRoleAsyncScenario
                    {
                        RoleId = Guid.NewGuid(),
                        TenantIds = null,
                        IsAdformAdmin = true,
                        HasVisibilityToRole = true
                    },
                    new CanEditRoleAsyncScenario
                    {
                        RoleId = Guid.NewGuid(),
                        TenantIds = null,
                        IsAdformAdmin = true,
                        HasVisibilityToRole = false
                    },
                    new CanEditRoleAsyncScenario
                    {
                        RoleId = Guid.NewGuid(),
                        TenantIds = null,
                        IsAdformAdmin = false,
                        HasVisibilityToRole = true
                    },
                    new CanEditRoleAsyncScenario
                    {
                        RoleId = Guid.NewGuid(),
                        TenantIds = null,
                        IsAdformAdmin = false,
                        HasVisibilityToRole = false
                    }
            };
            return data;
        }

        public static TheoryData<HasEnoughRoleAssignmentCapacityScenario> GenerateHasEnoughRoleAssignmentCapacityScenario()
        {
            var data = new TheoryData<HasEnoughRoleAssignmentCapacityScenario>
            {
                new HasEnoughRoleAssignmentCapacityScenario
                {
                    RoleId = Guid.NewGuid(),
                    Assignments = new List<RoleTenant>{new() {}},
                    Unassignments = null,
                    CurrentCount = 1,
                    Limit = 1,
                    Result = false
                    
                },
                new HasEnoughRoleAssignmentCapacityScenario
                {
                    RoleId = Guid.NewGuid(),
                    Unassignments = new List<RoleTenant>{new() {}},
                    Assignments = null,
                    CurrentCount = 1,
                    Limit = 1,
                    Result = true

                },
                new HasEnoughRoleAssignmentCapacityScenario
                {
                    RoleId = Guid.NewGuid(),
                    Unassignments = new List<RoleTenant>{new() {}},
                    Assignments = new List<RoleTenant>(),
                    CurrentCount = 1,
                    Limit = 1,
                    Result = true

                },
                new HasEnoughRoleAssignmentCapacityScenario
                {
                    RoleId = Guid.NewGuid(),
                    Assignments = new List<RoleTenant>{new() {}},
                    Unassignments = new List<RoleTenant>{new() {}},
                    CurrentCount = 1,
                    Limit = 1,
                    Result = true
                }
            };
            return data;
        }

        public class FilterFeatureIdsWithAccessDeniedScenario
        {
            public IReadOnlyCollection<Guid> InitialFeatureList { get; set; }
            public IReadOnlyCollection<Guid> DeniedFeatureList { get; set; }
            public Guid TenantId { get; set; } = Guid.NewGuid();
            public Guid RoleId { get; set; } = Guid.NewGuid();
            public Func<IReadOnlyCollection<Guid>, bool> Callback { get; set; }
        }

        public class CanAssignPermissionToRoleScenario
        {
            public bool DoesPermissionExist { get; set; }
            public bool DoesRoleExist { get; set; }
            public bool HasVisibilityToPermissionAsync { get; set; }
            public bool HasVisibilityToRoleAsync { get; set; }
            public Func<ValidationResult, bool> Callback { get; set; }
        }


        public class CanCreateSubjectScenario
        {
            public bool DoesSubjectExist { get; set; }
            public bool Callback { get; set; }
        }


        public class CanCreateRoleScenario
        {
            public bool DoesPolicyExist { get; set; }
            public bool DoesTenantExist { get; set; }
            public bool DoFeaturesExist { get; set; }
            public bool IsFeatureDependencySatisfied { get; set; }
            public bool HasVisibilityToFeature { get; set; }
            public bool IsTemplateRole { get; set; }
            public string PrincipalTenantId { get; set; }
            public string RoleName { get; set; }

            public Guid? PolicyId { get; set; } = Guid.NewGuid();
            public Guid TenantId { get; set; } = Guid.NewGuid();
            public Guid[] FeatureIds { get; set; } = new[] { Guid.NewGuid() };

            public Func<ValidationResult, bool> Callback { get; set; }
        }

        public class CanUpdateRoleScenario
        {
            public Guid RoleId { get; set; } = Guid.NewGuid();
            public Guid TenantId { get; set; } = Guid.NewGuid();
            public bool DoesRoleExist { get; set; }
            public bool HasVisibilityToRoleAsync { get; set; }
            public Func<ValidationResult, bool> Callback { get; set; }
        }

        public class HasEnoughRoleAssignmentCapacityScenario
        {
            public Guid RoleId { get; set; } = Guid.NewGuid();
            public IEnumerable<RoleTenant> Assignments { get; set; }
            public IEnumerable<RoleTenant> Unassignments { get; set; }
            public int CurrentCount { get; set; }
            public int Limit { get; set; }
            public bool Result { get; set; }
        }

        public class CanAssignRoleToFeaturesScenario
        {
            public bool DoesRoleExist { get; set; }
            public bool HasVisibilityToRole { get; set; }
            public bool HasVisibilityToFeatures { get; set; }
            public Func<ValidationResult, bool> Callback { get; set; }
        }

        public class CanAssignSubjectToRolesScenario
        {
            private static readonly Guid[] TenantIds = { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            public Guid[] ContextTenantIds => TenantIds;

            public IEnumerable<RoleTenant> Assignments { get; set; } =
                new List<RoleTenant>
                {
                    new RoleTenant()
                    {
                        TenantId = TenantIds[0],
                        RoleId = Guid.NewGuid()
                    },
                    new RoleTenant()
                    {
                        TenantId = TenantIds[1],
                        RoleId = Guid.NewGuid()
                    },
                    new RoleTenant()
                    {
                        TenantId = TenantIds[2],
                        RoleId = Guid.NewGuid()
                    }
                };
            public bool IsSameSubject { get; set; }
            public bool DoSubjectExist { get; set; }
            public bool DoTenantsExist { get; set; }
            public bool DoRolesExist { get; set; }
            public bool HasVisibilityToRolesAsync { get; set; }
            public bool HasEnoughRoleAssignmentCapacity { get; set; }
            public Func<ValidationResult, bool> Callback { get; set; }
        }

        public class CanCreateFeatureCoDepenencyScenario
        {
            public bool DoesFeatureExist { get; set; }
            public bool DoesDependsOnExist { get; set; }
            public bool HasVisibilityToFeature { get; set; }
            public bool HasVisibilityToDependentOn { get; set; }
            public bool IsAssignment { get; set; }
            public bool IsCircuralDependency { get; set; }
            public Func<ValidationResult, bool> Callback { get; set; }
        }

        public class CanUnassignSubjectFromRolesScenario
        {
            private static readonly Guid[] TenantIds = { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            public Guid[] ContextTenantIds => TenantIds;

            public IEnumerable<RoleTenant> Assignments { get; set; } =
                new List<RoleTenant>
                {
                    new RoleTenant()
                    {
                        TenantId = TenantIds[0],
                        RoleId = Guid.NewGuid()
                    },
                    new RoleTenant()
                    {
                        TenantId = TenantIds[1],
                        RoleId = Guid.NewGuid()
                    },
                    new RoleTenant()
                    {
                        TenantId = TenantIds[2],
                        RoleId = Guid.NewGuid()
                    }
                };

            public Guid SubjectId { get; set; }
            public bool IsSameSubject { get; set; }
            public bool DoSubjectExist { get; set; }
            public bool DoTenantsExist { get; set; }
            public bool DoRolesExist { get; set; }
            public bool HasVisibilityToRolesAsync { get; set; }
            public bool HasVisibilityToSubjectAsync { get; set; }
            public bool HasEnoughRoleAssignmentCapacity { get; set; }
            public Func<ValidationResult, bool> Callback { get; set; }
        }

        public class CanAssignTenantToFeatureAsyncScenario
        {
            public bool DoesTenantExist { get; set; }
            public bool DoesFeatureExist { get; set; }
            public bool HasVisibilityToTenant { get; set; }
            public bool HasTenantFeatureCoDependenciesConflict { get; set; }
            public bool IsAssignOperation { get; set; }
            public Func<ValidationResult, bool> Callback { get; set; }
        }

        public class CanAssignLicensedFeatureToTenantAsyncScenario
        {
            public bool DoesTenantExist { get; set; }
            public bool DoesLicensedFeatureExist { get; set; }
            public bool HasVisibilityToTenant { get; set; }
            public Func<ValidationResult, bool> Callback { get; set; }
        }

        public class CanEditRoleAsyncScenario
        {
            public Guid RoleId { get; set; } = Guid.NewGuid();
            public Guid[] TenantIds { get; set; }
            public  bool IsAdformAdmin { get; set; }
            public bool HasVisibilityToRole { get; set; }
        }
    }
}