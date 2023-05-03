using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Adapters;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Validations;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Unit.Test.Domain;
using Bogus;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.DataAccess;

public class ValidatorAdapterTests
{
    private readonly Mock<IAdminGraphRepository> _graphRepository;
    private readonly Mock<IVisibilityProvider<QueryParamsRoles, Contracts.Output.Role>> _roleVisibilityProvider;
    private readonly IOptions<ValidationConfiguration> _validationConfiguration;
    private readonly ValidatorAdapter _validatorAdapter;
    private readonly Faker<Group> _groupFaker;

    public ValidatorAdapterTests()
    {
        _graphRepository = new Mock<IAdminGraphRepository>();
        _roleVisibilityProvider = new Mock<IVisibilityProvider<QueryParamsRoles, Contracts.Output.Role>>();
        _validationConfiguration = Options.Create(new ValidationConfiguration());
        _validatorAdapter = new ValidatorAdapter(_graphRepository.Object, _roleVisibilityProvider.Object,
            It.IsAny<IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Subject>>(),
            It.IsAny<IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Permission>>(),
            It.IsAny<IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature>>(),
            _validationConfiguration);
        _groupFaker = new Faker<Group>()
            .RuleForType(typeof(Guid), _ => Guid.NewGuid());
    }

    [Theory]
    [MemberData(nameof(Scenarios.GenerateCanEditRoleAsyncScenario), MemberType = typeof(Scenarios))]
    public async Task CanEditRole_Returns_ExpectedResult(Scenarios.CanEditRoleAsyncScenario s)
    {
        //Arrange
        var claimRole = s.IsAdformAdmin ? Graph.AdformAdminRoleName : Graph.Subject1;
        var principal = Bloom.Common.Test.Common.BuildUser(
            new RuntimeResponse
            {
                Roles = new[] {claimRole}
            }
        );
        var tenantId = new Guid();
        _roleVisibilityProvider.Setup(m => m.HasVisibilityAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<QueryParamsRoles>(),
                It.IsAny<string>()))
            .ReturnsAsync(s.HasVisibilityToRole);
        var tenantIds = new List<Guid> {tenantId};
        _graphRepository
            .Setup(o => o.GetConnectedAsync<Role, Tenant>(It.IsAny<Expression<Func<Role, bool>>>(),
                Constants.OwnsIncomingLink))
            .ReturnsAsync(new List<Tenant> {new() {Id = tenantId}});

        //Act
        var result = await _validatorAdapter.CanEditRoleAsync(principal, s.RoleId);

        //Assert
        Assert.Equal(result, s.IsAdformAdmin || s.HasVisibilityToRole);
        if (s.IsAdformAdmin)
            _roleVisibilityProvider.Verify(m => m.HasVisibilityAsync(It.IsAny<ClaimsPrincipal>(),
                It.IsAny<QueryParamsRoles>(),
                It.IsAny<string>()), Times.Never);
        else
            _roleVisibilityProvider.Verify(m => m.HasVisibilityAsync(It.IsAny<ClaimsPrincipal>(),
                It.Is<QueryParamsRoles>(p => p.TenantIds.SequenceEqual(tenantIds)),
                Constants.Label.CUSTOM_ROLE), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Scenarios.GenerateHasEnoughRoleAssignmentCapacityScenario), MemberType = typeof(Scenarios))]
    public async Task HasEnoughRoleAssignmentCapacity_Returns_ExpectedResult(
        Scenarios.HasEnoughRoleAssignmentCapacityScenario s)
    {
        //Arrange
        var subjectId = Guid.NewGuid();
        _graphRepository.Setup(m => m.GetConnectedAsync<Subject, Group>(s => s.Id == subjectId, Constants.MemberOfLink))
            .ReturnsAsync(_groupFaker.Generate(s.CurrentCount));
        _validationConfiguration.Value.RoleLimitPerSubject = s.Limit;

        //Act
        var result = await _validatorAdapter.HasEnoughRoleAssignmentCapacityAsync(subjectId, s.Assignments, s.Unassignments);

        //Assert
        Assert.Equal(s.Result, result);
    }
}