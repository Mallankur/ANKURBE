using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Api.Services;
using Adform.Bloom.Client.Contracts;
using Adform.Bloom.Runtime.Contracts.Request;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Runtime.Contracts.Services;
using Bogus;
using IdentityServer4.Extensions;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Api.Services;

public class ClaimPrincipalGeneratorTests
{

    private readonly Mock<IBloomRuntimeClient> _client;
    public ClaimPrincipalGeneratorTests()
    {
        _client = new Mock<IBloomRuntimeClient>();
    }

    [Theory]
    [MemberData(nameof(CreateScenario), MemberType = typeof(ClaimPrincipalGeneratorTests))]
    public async Task GenerateAsync_Creates_ClaimPrincipal(Guid subjectId, IReadOnlyCollection<RuntimeResponse> runtimeResult)
    {
        // Arrange
        _client.Setup(o => o.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(runtimeResult);
        var generator = new ClaimPrincipalGenerator(_client.Object);
        // Act
        var result = await generator.GenerateAsync(subjectId, null, CancellationToken.None);
        // Assert
        Assert.Equal(2, result.Identities.Count());

        _client.Verify(o => o.InvokeAsync(
            It.Is<SubjectRuntimeRequest>(p => p.SubjectId == subjectId
                                              && p.TenantIds.IsNullOrEmpty()),
            It.IsAny<CancellationToken>()), Times.Once);
        var identity = result.Identities.FirstOrDefault(p => p.AuthenticationType == Authentication.Bloom);
        Assert.NotNull(identity);
        var roles = identity.Claims.Where(o => o.Type == "role").Select(p => p.Value).OrderBy(o => o).ToList();
        Assert.Equal(runtimeResult.SelectMany(o => o.Roles).Distinct().OrderBy(p => p),
            roles.Distinct().OrderBy(p => p));
        var permissions = identity.Claims.Where(o => o.Type == "permission").Select(p => p.Value).OrderBy(o => o)
            .ToList();
        Assert.Equal(runtimeResult.SelectMany(o => o.Permissions).Distinct().OrderBy(p => p),
            permissions.Distinct().OrderBy(p => p));
    }

    [Theory]
    [MemberData(nameof(CreateScenario), MemberType = typeof(ClaimPrincipalGeneratorTests))]
    public async Task GenerateAsync_Creates_ClaimPrincipal_FilteredBy_ActorPrincipal(Guid subjectId, IReadOnlyCollection<RuntimeResponse> subjectRuntime)
    {
        // Arrange
        var actorId = Guid.NewGuid();
        var rnd = new Random();
        var actorRuntime = subjectRuntime.Take(rnd.Next(subjectRuntime.Count));
        _client.SetupSequence(o => o.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(actorRuntime)
            .ReturnsAsync(subjectRuntime);
        var generator = new ClaimPrincipalGenerator(_client.Object);
        // Act
        var actorPrincipal = await generator.GenerateAsync(actorId, null, CancellationToken.None);
        var result = await generator.GenerateAsync(subjectId, actorPrincipal, CancellationToken.None);
        // Assert
        Assert.Equal(2, result.Identities.Count());

        _client.Verify(o => o.InvokeAsync(
            It.Is<SubjectRuntimeRequest>(p => p.SubjectId == subjectId 
                                              && p.TenantIds.IsNullOrEmpty()),
            It.IsAny<CancellationToken>()), Times.Once);
        var identity = result.Identities.FirstOrDefault(p => p.AuthenticationType == Authentication.Bloom);
        Assert.NotNull(identity);
        var roles = identity.Claims.Where(o => o.Type == "role").Select(p => p.Value).OrderBy(o => o).ToList();
        Assert.Equal(actorRuntime.SelectMany(o => o.Roles).Distinct().OrderBy(p => p),
            roles.Distinct().OrderBy(p => p));
        var permissions = identity.Claims.Where(o => o.Type == "permission").Select(p => p.Value).OrderBy(o => o)
            .ToList();
        Assert.Equal(actorRuntime.SelectMany(o => o.Permissions).Distinct().OrderBy(p => p),
            permissions.Distinct().OrderBy(p => p));
    }

    public static TheoryData<Guid, IReadOnlyCollection<RuntimeResponse>> CreateScenario()
    {
        var roles = new List<string> { "roles0", "roles1", "roles2", "roles3" };
        var permissions = new List<string>
        {
            "permission0", "permission1", "permission2", "permission3", "permission4", "permission5", "permission6"
        };
        var data = new TheoryData<Guid, IReadOnlyCollection<RuntimeResponse>>();
        var fakeResult = new Faker<RuntimeResponse>();
        fakeResult.RuleFor(p => p.TenantId, o => Guid.NewGuid());
        fakeResult.RuleFor(p => p.TenantName, o => o.Company.CompanyName());
        fakeResult.RuleFor(p => p.Roles, o => o.PickRandom(roles, 2).ToList());
        fakeResult.RuleFor(p => p.Permissions, o => o.PickRandom(permissions, 3).ToList());
        var subjects = Enumerable.Range(0, 5)
            .Select(n => new Faker().Random.Guid())
            .ToList();

        foreach (var t in subjects)
        {
            var result = fakeResult.Generate(subjects.Count);
            data.Add(t, result);
        }

        return data;
    }
}