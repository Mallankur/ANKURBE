using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Client.Contracts.Request;
using Adform.Bloom.Client.Contracts.Response;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Adform.Bloom.Client.Contracts.Services
{
    public class BloomRuntimeClient : IBloomRuntimeClient
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly HttpClient _client;

        public BloomRuntimeClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<RuntimeResponse>> InvokeAsync(SubjectRuntimeRequest data, CancellationToken cancellationToken = default)
        {
            return await CallWithJsonInput("/v1/runtime/subject-evaluation", data, () => Array.Empty<RuntimeResponse>(), cancellationToken);
        }

        public async Task<IEnumerable<RuntimeResponse>> IntersectionAsync(SubjectIntersectionRequest data, CancellationToken cancellationToken = default)
        {
            return await CallWithJsonInput("/v1/runtime/subject-intersection", data, () => Array.Empty<RuntimeResponse>(), cancellationToken);
        }

        public async Task<ExistenceResponse> LegacyTenantExistenceAsync(LegacyTenantExistenceRequest request, CancellationToken cancellationToken = default)
        {
            return await CallWithJsonInput("/v1/runtime/legacy-tenants-exist", request, () => new ExistenceResponse { Exists = false }, cancellationToken);
        }

        public async Task<ExistenceResponse> RoleExistenceAsync(RoleExistenceRequest request, CancellationToken cancellationToken = default)
        {
            return await CallWithJsonInput("/v1/runtime/role-exists", request, () => new ExistenceResponse { Exists = false }, cancellationToken);
        }

        public async Task<ExistenceResponse> NodeExistenceAsync(NodeExistenceRequest request, CancellationToken cancellationToken = default)
        {
            return await CallWithJsonInput("/v1/runtime/nodes-exist", request, () => new ExistenceResponse { Exists = false }, cancellationToken);
        }

        private async Task<TResult> CallWithJsonInput<TInput, TResult>(string uri, TInput data, Func<TResult> defaultResult, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var input = JsonSerializer.Serialize(data);
            request.Content = new StringContent(input, Encoding.UTF8, "application/json");

            using var response = await _client.SendAsync(request, cancellationToken);
            if (response == null) return defaultResult();
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TResult>(jsonString, Options);
            return result ?? defaultResult();
        }

        public async Task<bool> IsHealthy()
        {
            var response = await _client.GetAsync("/healthy");
            var res = await response.Content.ReadAsStringAsync();
            return string.Equals(res, HealthStatus.Healthy.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}