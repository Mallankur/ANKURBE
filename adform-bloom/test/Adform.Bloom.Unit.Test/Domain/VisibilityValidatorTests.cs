using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Ports;
using Adform.Bloom.Domain.Validations;
using Adform.Bloom.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Domain
{
    public class VisibilityValidatorTests
    {
        private const string Tenant = "tenant";

        private readonly Mock<IRoleValidator> _roleProvider = new Mock<IRoleValidator>();
        private readonly Mock<ISubjectValidator> _subjectProvider = new Mock<ISubjectValidator>();
        private readonly Mock<ITenantValidator> _tenantProvider = new Mock<ITenantValidator>();
        private readonly Mock<IPolicyValidator> _policyProvider = new Mock<IPolicyValidator>();
        private readonly Mock<IPermissionValidator> _permissionProvider = new Mock<IPermissionValidator>();
        private readonly Mock<IFeatureValidator> _featureProvider = new Mock<IFeatureValidator>();
        private readonly Mock<ILicensedFeatureValidator> _licensedFeatureProvider = new Mock<ILicensedFeatureValidator>();

        private readonly ClaimsPrincipal _principal = Common.BuildPrincipal(Tenant);
        private readonly AccessValidator _validator;

        public VisibilityValidatorTests()
        {
            _validator = new AccessValidator(
                _roleProvider.Object,
                _subjectProvider.Object,
                _tenantProvider.Object,
                _policyProvider.Object,
                _permissionProvider.Object,
                _featureProvider.Object,
                _licensedFeatureProvider.Object);
        }

        [Theory]
        [InlineData(ErrorCodes.RoleDoesNotExist)]
        [InlineData(ErrorCodes.SubjectCannotAccessRole)]
        public async Task CanDeleteRoleAsync_Returns_Error(ErrorCodes errorCode)
        {
            var roleId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            _roleProvider.Setup(x => x.DoesRoleExist(roleId, null)).ReturnsAsync(errorCode != ErrorCodes.RoleDoesNotExist);
            _roleProvider.Setup(m => m.CanEditRoleAsync(It.Is<ClaimsPrincipal>(c => c.Claims.Any(cl => cl.Issuer == Tenant)), roleId)).ReturnsAsync(errorCode != ErrorCodes.SubjectCannotAccessRole);

            var result = await _validator.CanDeleteRoleAsync(_principal, roleId);
            
            Assert.Equal(errorCode, result.Error);

            _roleProvider.Verify(p => p.DoesRoleExist(roleId, It.IsAny<string>()), Times.Once);
            _roleProvider.Verify(p => p.CanEditRoleAsync(It.Is<ClaimsPrincipal>(c => c.Claims.Any(cl => cl.Issuer == Tenant)),
                roleId), errorCode.Equals(ErrorCodes.RoleDoesNotExist) ? Times.Never : Times.Once);
        }

        [Fact]
        public async Task CanDeleteRoleAsync_Returns_Success()
        {
            var roleId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            _roleProvider.Setup(x => x.DoesRoleExist(roleId, null)).ReturnsAsync(true);
            _roleProvider.Setup(m => m.CanEditRoleAsync(It.Is<ClaimsPrincipal>(c => c.Claims.Any(cl => cl.Issuer == Tenant)), roleId)).ReturnsAsync(true);

            var result = await _validator.CanDeleteRoleAsync(_principal, roleId);

            Assert.False(result.HasError(ErrorCodes.RoleDoesNotExist));
            Assert.False(result.HasError(ErrorCodes.SubjectCannotAccessRole));

            _roleProvider.Verify(p => p.DoesRoleExist(roleId, It.IsAny<string>()), Times.Once);
            _roleProvider.Verify(p => p.CanEditRoleAsync(It.Is<ClaimsPrincipal>(c => c.Claims.Any(cl => cl.Issuer == Tenant)), roleId), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CanDeleteSubjectAsync_Returns_Expected_Result(bool result)
        {
            var guid = Guid.NewGuid();

            _subjectProvider.Setup(m =>
                    m.HasVisibilityToSubjectAsync(
                        It.Is<ClaimsPrincipal>(c => c.Claims.Any(cl => cl.Issuer == Tenant)),
                        guid,
                        null))
                .ReturnsAsync(result);

            Assert.Equal(result, await _validator.CanDeleteSubjectAsync(_principal, guid));
        }

        [Theory]
        [MemberData(nameof(Scenarios.GenerateCanAssignPermissionToRoleScenarios), MemberType = typeof(Scenarios))]
        public async Task CanAssignPermissionToRoleAsync_Returns_Expected_Results(Scenarios.CanAssignPermissionToRoleScenario s)
        {
            var permissionId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();

            _permissionProvider.Setup(m => m.DoesPermissionExist(permissionId)).ReturnsAsync(s.DoesPermissionExist);
            _permissionProvider.Setup(m => m.HasVisibilityToPermissionAsync(_principal, permissionId, new[] { tenantId })).ReturnsAsync(s.HasVisibilityToPermissionAsync);

            _roleProvider.Setup(m => m.DoesRoleExist(roleId, null)).ReturnsAsync(s.DoesRoleExist);
            _roleProvider.Setup(m => m.GetRoleOwner(roleId)).ReturnsAsync(tenantId);
            _roleProvider.Setup(m => m.CanEditRoleAsync(_principal, roleId)).ReturnsAsync(s.HasVisibilityToRoleAsync);

            var result = await _validator.CanAssignPermissionToRoleAsync(_principal, permissionId, roleId);
            Assert.True(s.Callback(result));
            _roleProvider.Verify(p => p.DoesRoleExist(roleId, It.IsAny<string>()), Times.Once);
            _roleProvider.Verify(p => p.GetRoleOwner(roleId), Times.Once);
            _roleProvider.Verify(p => p.CanEditRoleAsync(_principal, roleId), Times.Once);
            _permissionProvider.Verify(p=> p.DoesPermissionExist(permissionId), Times.Once);
            _permissionProvider.Verify(p=> p.HasVisibilityToPermissionAsync(_principal, permissionId, new[] { tenantId }), Times.Once);
        }

        [Theory]
        [MemberData(nameof(Scenarios.GenerateCanCreateRoleScenario), MemberType = typeof(Scenarios))]
        public async Task CanCreateRoleAsync_Returns_Expected_Results(Scenarios.CanCreateRoleScenario s)
        {
            var principal = Common.BuildPrincipal(s.PrincipalTenantId ?? s.TenantId.ToString());

            if (s.PolicyId != null)
                _policyProvider.Setup(m => m.DoesPolicyExist(s.PolicyId.Value)).ReturnsAsync(s.DoesPolicyExist);

            _tenantProvider.Setup(m => m.DoesTenantExist(s.TenantId)).ReturnsAsync(s.DoesTenantExist);
            _featureProvider.Setup(m => m.DoFeaturesExist(s.FeatureIds)).ReturnsAsync(s.DoFeaturesExist);
            _featureProvider.Setup(m => m.CoDependencyFeaturesSelected(s.FeatureIds))
                .ReturnsAsync(s.IsFeatureDependencySatisfied);

            if (s.FeatureIds != null)
                _featureProvider.Setup(m => m.HasVisibilityToFeaturesAsync(principal, s.FeatureIds, new[] { s.TenantId }))
                    .ReturnsAsync(s.HasVisibilityToFeature);

            if (s.IsTemplateRole && s.RoleName != null)
            {
                var roleClaim = new Claim("role", s.RoleName, "json", Guid.NewGuid().ToString());
                var claimsIdentity = (ClaimsIdentity)principal.Identity;
                claimsIdentity.AddClaim(roleClaim);
            }

            var result =
                await _validator.CanCreateRole(principal, s.PolicyId, s.TenantId, s.FeatureIds, s.IsTemplateRole);

            Assert.True(s.Callback(result));
        }

        [Theory]
        [MemberData(nameof(Scenarios.GenerateCanUpdateRoleScenario), MemberType = typeof(Scenarios))]
        public async Task CanUpdateRoleAsync_Returns_Expected_Results(Scenarios.CanUpdateRoleScenario s)
        {
            var principal = Common.BuildPrincipal(s.TenantId.ToString());
            _roleProvider.Setup(m => m.DoesRoleExist(s.RoleId, It.IsAny<string>())).ReturnsAsync(s.DoesRoleExist);
            _roleProvider.Setup(m => m.CanEditRoleAsync(
                It.Is<ClaimsPrincipal>(c => c.Claims.Any(cl => cl.Issuer == s.TenantId.ToString())), s.RoleId)).ReturnsAsync(s.HasVisibilityToRoleAsync);

            var result = await _validator.CanUpdateRole(principal, s.RoleId);

            Assert.True(s.Callback(result));
            _roleProvider.Verify(p => p.DoesRoleExist(s.RoleId, It.IsAny<string>()), Times.Once);
            _roleProvider.Verify(p => p.CanEditRoleAsync(principal, s.RoleId), Times.Once);
        }

        [Theory]
        [MemberData(nameof(Scenarios.GenerateCanCreateSubjectScenario), MemberType = typeof(Scenarios))]
        public async Task CanCreateSubjectAsync_Returns_Expected_Results(Scenarios.CanCreateSubjectScenario s)
        {
            // Arrange
            var principal = Common.BuildPrincipal();
            _subjectProvider.Setup(x => x.SubjectExists(It.IsAny<Guid>()))
                .ReturnsAsync(s.DoesSubjectExist);

            // Act

            var result = await _validator.CanCreateSubjectAsync(principal, Guid.NewGuid());

            // Assert
            Assert.Equal(s.Callback, result);
        }

        [Theory]
        [MemberData(nameof(Scenarios.GenerateCanAssignSubjectToRolesScenario), MemberType = typeof(Scenarios))]
        public async Task CanAssignSubjectToRolesAsync_Returns_Expected_Results(Scenarios.CanAssignSubjectToRolesScenario s)
        {
            var principal = Common.BuildPrincipal(s.ContextTenantIds);
            _subjectProvider.Setup(x => x.IsSameSubject(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()))
                .ReturnsAsync(s.IsSameSubject);
            _subjectProvider.Setup(x => x.SubjectExists(It.IsAny<Guid>()))
                .ReturnsAsync(s.DoSubjectExist);
            _tenantProvider.Setup(m =>
                    m.DoTenantsExist(
                        It.Is<Guid[]>(x => x.SequenceEqual(s.Assignments.Select(y => y.TenantId).ToArray()))))
                .ReturnsAsync(s.DoTenantsExist);
            _roleProvider.Setup(m =>
                    m.DoRolesExist(It.Is<Guid[]>(x => x.SequenceEqual(s.Assignments.Select(y => y.RoleId).ToArray()))))
                .ReturnsAsync(s.DoRolesExist);
            _roleProvider.Setup(m => m.HasVisibilityToRolesAsync(
                    It.Is<ClaimsPrincipal>(c => c.Claims.Any(cl => s.ContextTenantIds.Contains(Guid.Parse(cl.Issuer)))),
                    It.Is<Guid[]>(x => x.SequenceEqual(s.Assignments.Select(y => y.RoleId).ToArray())),
                    It.Is<Guid[]>(x => x.SequenceEqual(s.Assignments.Select(y => y.TenantId).ToArray()))))
                .ReturnsAsync(s.HasVisibilityToRolesAsync);
            _subjectProvider.Setup(m =>
                    m.HasEnoughRoleAssignmentCapacityAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>(), It.IsAny<IEnumerable<RoleTenant>>()))
                .ReturnsAsync(s.HasEnoughRoleAssignmentCapacity);

            var result = await _validator.CanAssignSubjectToRolesAsync(principal, s.Assignments, s.Assignments, null, Guid.NewGuid());

            Assert.True(s.Callback(result));
        }

        [Theory]
        [MemberData(nameof(Scenarios.GenerateCanAssignRoleToFeaturesScenario), MemberType = typeof(Scenarios))]
        public async Task CanAssignRoleToFeaturesAsync_Returns_Expected_Results(Scenarios.CanAssignRoleToFeaturesScenario s)
        {
            var tenantId = Guid.NewGuid();
            var principal = Common.BuildPrincipal(tenantId.ToString());
            var roleId = Guid.NewGuid();
            var featureIds = new[]
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            };
            _roleProvider.Setup(m => m.GetRoleOwner(roleId)).ReturnsAsync(tenantId);
            _roleProvider.Setup(m => m.DoesRoleExist(roleId, It.IsAny<string>())).ReturnsAsync(s.DoesRoleExist);
            _roleProvider.Setup(m => m.CanEditRoleAsync(principal, roleId)).ReturnsAsync(s.HasVisibilityToRole);

            _featureProvider.Setup(x => x.HasVisibilityToFeaturesAsync(principal, featureIds, It.IsAny<IReadOnlyCollection<Guid>>())).ReturnsAsync(s.HasVisibilityToFeatures);

            var result = await _validator.CanAssignRoleToFeaturesAsync(principal, roleId, featureIds);

            Assert.True(s.Callback(result));
            _roleProvider.Verify(p => p.GetRoleOwner(roleId), Times.Once);
            _roleProvider.Verify(p => p.DoesRoleExist(roleId, It.IsAny<string>()), Times.Once);
            _roleProvider.Verify(p => p.CanEditRoleAsync(principal, roleId), Times.Once);
        }
        [Theory]
        [MemberData(nameof(Scenarios.GenerateFilterFeatureIdsWithAccessDeniedScenario), MemberType = typeof(Scenarios))]
        public async Task FilterFeatureIdsWithAccessDeniedAsync_Returns_Expected_Results(Scenarios.FilterFeatureIdsWithAccessDeniedScenario s)
        {
            var principal = Common.BuildPrincipal(s.TenantId.ToString());
            _roleProvider.Setup(r => r.GetRoleOwner(s.RoleId)).ReturnsAsync(s.TenantId);
            _featureProvider
                .Setup(f => f.FilterFeatureIdsWithAccessDeniedAsync(principal, s.InitialFeatureList, new[] {s.TenantId}))
                .ReturnsAsync(s.DeniedFeatureList);

            var result = await _validator.FilterFeatureIdsWithAccessDeniedAsync(principal, s.RoleId, s.InitialFeatureList);

            Assert.True(s.Callback(result));
        }

        [Theory]
        [MemberData(nameof(Scenarios.GenerateCanCreateFeatureCoDepenencyScenario), MemberType = typeof(Scenarios))]
        public async Task CanCreateFeatureCoDependency_Returns_Expected_Results(
            Scenarios.CanCreateFeatureCoDepenencyScenario s)
        {
            var principal = Common.BuildPrincipal(Guid.NewGuid().ToString());
            var featureId = Guid.NewGuid();
            var dependentOnId = Guid.NewGuid();

            _featureProvider
                .Setup(x => x.HasVisibilityToFeaturesAsync(principal,
                    It.Is<Guid[]>(y => y.SequenceEqual(new[] { featureId })), null)).ReturnsAsync(s.HasVisibilityToFeature);
            _featureProvider
                .Setup(x => x.HasVisibilityToFeaturesAsync(principal,
                    It.Is<Guid[]>(y => y.SequenceEqual(new[] { dependentOnId })), null))
                .ReturnsAsync(s.HasVisibilityToDependentOn);
            _featureProvider.Setup(x => x.IsFeatureDependentOnOtherFeature(featureId, dependentOnId))
                .ReturnsAsync(s.IsCircuralDependency);
            _featureProvider.Setup(x =>
                    x.DoFeaturesExist(It.Is<IReadOnlyCollection<Guid>>(y => y.SequenceEqual(new[] { featureId }))))
                .ReturnsAsync(s.DoesFeatureExist);
            _featureProvider.Setup(x =>
                    x.DoFeaturesExist(It.Is<IReadOnlyCollection<Guid>>(y => y.SequenceEqual(new[] { dependentOnId }))))
                .ReturnsAsync(s.DoesDependsOnExist);

            var result =
                await _validator.CanCreateFeatureCoDependency(principal, featureId, dependentOnId, s.IsAssignment);

            Assert.True(s.Callback(result));
        }

        [Theory]
        [MemberData(nameof(Scenarios.GenerateCanUnassignSubjectFromRolesScenario), MemberType = typeof(Scenarios))]
        public async Task CanUnassignSubjectToRolesAsync_Returns_Expected_Results(
            Scenarios.CanUnassignSubjectFromRolesScenario s)
        {
            var principal = Common.BuildPrincipal(s.ContextTenantIds);
            _tenantProvider.Setup(m =>
                    m.DoTenantsExist(
                        It.Is<Guid[]>(x => x.SequenceEqual(s.Assignments.Select(y => y.TenantId).ToArray()))))
                .ReturnsAsync(s.DoTenantsExist);
            _roleProvider.Setup(m =>
                    m.DoRolesExist(It.Is<Guid[]>(x => x.SequenceEqual(s.Assignments.Select(y => y.RoleId).ToArray()))))
                .ReturnsAsync(s.DoRolesExist);
            _roleProvider.Setup(m => m.HasVisibilityToRolesAsync(
                    It.Is<ClaimsPrincipal>(c => c.Claims.Any(cl => s.ContextTenantIds.Contains(Guid.Parse(cl.Issuer)))),
                    It.Is<Guid[]>(x => x.SequenceEqual(s.Assignments.Select(y => y.RoleId).ToArray())),
                    It.Is<Guid[]>(x => x.SequenceEqual(s.Assignments.Select(y => y.TenantId).ToArray()))))
                .ReturnsAsync(s.HasVisibilityToRolesAsync);
            _subjectProvider.Setup(x => x.IsSameSubject(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()))
                .ReturnsAsync(s.IsSameSubject);
            _subjectProvider.Setup(x => x.SubjectExists(It.IsAny<Guid>()))
                .ReturnsAsync(s.DoSubjectExist);
            _subjectProvider
                .Setup(m => m.HasVisibilityToSubjectAsync(
                    It.Is<ClaimsPrincipal>(c => c.Claims.Any(cl => s.ContextTenantIds.Contains(Guid.Parse(cl.Issuer)))),
                    s.SubjectId,
                    It.Is<Guid[]>(x => x.SequenceEqual(s.Assignments.Select(y => y.TenantId).ToArray()))))
                .ReturnsAsync(s.HasVisibilityToSubjectAsync);
            _subjectProvider.Setup(m =>
                    m.HasEnoughRoleAssignmentCapacityAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>(), It.IsAny<IEnumerable<RoleTenant>>()))
                .ReturnsAsync(s.HasEnoughRoleAssignmentCapacity);

            var result = await _validator.CanUnassignSubjectFromRolesAsync(principal, s.Assignments, s.SubjectId);

            Assert.True(s.Callback(result));
        }

        [Theory]
        [MemberData(nameof(Scenarios.GenerateCanAssignTenantToFeatureAsyncScenario), MemberType = typeof(Scenarios))]
        public async Task CanAssignTenantToFeatureAsync_Returns_Expected_Results(
            Scenarios.CanAssignTenantToFeatureAsyncScenario s)
        {
            var featureId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var principal =
                Common.BuildPrincipal(s.HasVisibilityToTenant ? tenantId.ToString() : Guid.NewGuid().ToString());
            _tenantProvider.Setup(m => m.DoesTenantExist(tenantId)).ReturnsAsync(s.DoesTenantExist);
            _featureProvider.Setup(x =>
                    x.DoFeaturesExist(It.Is<IReadOnlyCollection<Guid>>(y => y.SequenceEqual(new[] { featureId }))))
                .ReturnsAsync(s.DoesFeatureExist);
            _featureProvider.Setup(x => x.IsTenantAssignedToFeatureCoDependenciesAsync(tenantId, featureId))
                .ReturnsAsync(!s.HasTenantFeatureCoDependenciesConflict);
            _featureProvider.Setup(x => x.IsTenantAssignedToFeatureWithoutDependants(tenantId, featureId))
                .ReturnsAsync(!s.HasTenantFeatureCoDependenciesConflict);

            var result = await (s.IsAssignOperation
                ? _validator.CanAssignTenantToFeatureAsync(principal, tenantId, featureId)
                : _validator.CanUnassignTenantFromFeatureAsync(principal, tenantId, featureId));

            Assert.True(s.Callback(result));
        }

        [Theory]
        [MemberData(nameof(Scenarios.GenerateCanAssignLicensedFeatureToTenantAsyncScenario),
            MemberType = typeof(Scenarios))]
        public async Task CanAssignLicensedFeatureToTenantAsync_Returns_Expected_Results(
            Scenarios.CanAssignLicensedFeatureToTenantAsyncScenario s)
        {
            var licensedFeature = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var principal =
                Common.BuildPrincipal(s.HasVisibilityToTenant ? tenantId.ToString() : Guid.NewGuid().ToString());
            _tenantProvider.Setup(m => m.DoTenantsExist(new Guid[] { tenantId })).ReturnsAsync(s.DoesTenantExist);
            _licensedFeatureProvider.Setup(x =>
                    x.DoLicensedFeaturesExist(
                        It.Is<IReadOnlyCollection<Guid>>(y => y.SequenceEqual(new[] { licensedFeature }))))
                .ReturnsAsync(s.DoesLicensedFeatureExist);

            var result =
                await _validator.CanAssignLicensedFeatureToTenantAsync(principal, tenantId,
                    new Guid[] { licensedFeature });

            Assert.True(s.Callback(result));
        }
    }
}