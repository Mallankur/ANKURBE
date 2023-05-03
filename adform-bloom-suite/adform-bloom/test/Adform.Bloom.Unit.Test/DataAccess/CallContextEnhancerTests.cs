using System;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Ciam.TokenProvider.Configuration;
using Adform.Ciam.TokenProvider.Services;
using Bogus;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.DataAccess;

public class CallContextEnhancerTests
{
    private readonly Mock<ITokenProvider> _repositoryMock;
    private IOptions<BloomReadClientSettings> _options;

    public CallContextEnhancerTests()
    {
        _repositoryMock = new Mock<ITokenProvider>();
    }


    [Fact]
    public async Task Build_Sets_RequestHeaders()
    {
        // Arrange
        var fakerClient = new Faker<BloomReadClientSettings>()
            .RuleFor(o => o.Host, f => f.Internet.Ip())
            .RuleFor(o => o.Scopes, f => new[] { f.Lorem.Word() });
        var fakerOauth = new Faker<OAuth2Configuration>()
            .RuleFor(o => o.Authority, f => f.Internet.Ip())
            .RuleFor(o => o.ClientId, f => f.Person.Email)
            .RuleFor(o => o.ClientSecret, f => f.Lorem.Word());
        var options = fakerOauth.Generate();
        var settings = fakerClient.Generate();
        var token = Guid.NewGuid().ToString();

        _repositoryMock.Setup(o => o.RequestTokenAsync(options.ClientId, settings.Scopes)).ReturnsAsync(token);
        var ctxEnhancer = new CallContextEnhancer(_repositoryMock.Object, Options.Create(options), Options.Create(settings));
        // Act
        var ctx = await ctxEnhancer.Build();
        // Assert
        Assert.Equal(1, ctx.RequestHeaders.Count);
        Assert.Equal($"Bearer {token}", ctx.RequestHeaders[0].Value);
    }
}