using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Client.Contracts;
using Adform.Bloom.Client.Contracts.Request;
using Adform.Bloom.Client.Contracts.Response;
using Adform.Bloom.Client.Contracts.Services;
using Adform.Bloom.Runtime.Read.Entities;
using Adform.Ciam.ExceptionHandling.Abstractions;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using CorrelationId;
using CorrelationId.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Adform.Bloom.Runtime.Read.Test.Client;

public class BloomRuntimeClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    private readonly IBloomRuntimeClient _client;

    public BloomRuntimeClientTests()
    {
        _client = new BloomRuntimeClient(new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://test.com/")
        });
    }

    [Fact]
    public async Task Intersection_Returns_PolicyResult()
    {
        //Arrange
        const string clientName = "BloomRuntimeClient";
        var subjectQuery = new SubjectIntersectionRequest();

        var server = Common.ConfigureAndStartMockServer("/v1/runtime/subject-intersection", new List<RuntimeResult>());
        var serviceProvider = Common.CreateServiceProviderForRuntimeClient(clientName, server.Urls[0]);
        var client =
            new BloomRuntimeClient(serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(clientName));

        //Act
        var result = await client.IntersectionAsync(subjectQuery);

        //Assert
        var entries = server.LogEntries.First();
        Assert.Equal(entries.RequestMessage.Body, JsonSerializer.Serialize(subjectQuery));
        Assert.IsAssignableFrom<IEnumerable<RuntimeResponse>>(result);
    }

    [Fact]
    public async Task BloomRuntimeClient_Request_Contains_Passed_CorrelationId()
    {
        //Arrange
        const string clientName = "BloomRuntimeClient";
        var expectedCorrelationId = Guid.NewGuid().ToString();

        var server = Common.ConfigureAndStartMockServer("/v1/runtime/subject-evaluation", new List<RuntimeResult>());
        var serviceProvider = Common.CreateServiceProviderForRuntimeClient(clientName, server.Urls[0]);

        serviceProvider.GetRequiredService<ICorrelationContextAccessor>()
            .CorrelationContext = new CorrelationContext(expectedCorrelationId, Headers.CorrelationId);

        //Act
        await new BloomRuntimeClient(serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(clientName))
            .InvokeAsync(new SubjectRuntimeRequest(), CancellationToken.None);

        //Assert
        var entries = server.LogEntries.First();
        Assert.True(entries.RequestMessage.Headers.TryGetValue(Headers.CorrelationId, out var correlationIds));
        Assert.Matches(expectedCorrelationId, correlationIds.First());
    }
}