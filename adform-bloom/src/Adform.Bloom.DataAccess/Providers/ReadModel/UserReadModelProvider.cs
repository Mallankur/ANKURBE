using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts;
using Adform.Bloom.Read.Contracts.User;
using Adform.Ciam.SharedKernel.Entities;

namespace Adform.Bloom.DataAccess.Providers.ReadModel
{
    public class UserReadModelProvider : IUserReadModelProvider
    {
        private readonly ICallContextEnhancer _ctxEnhancer;
        private readonly IUserService _client;

        public UserReadModelProvider(IUserService client, ICallContextEnhancer ctxEnhancer)
        {
            _client = client;
            _ctxEnhancer = ctxEnhancer;
        }

        public async Task<User?> SearchForResourceAsync(Guid id, CancellationToken ct = default)
        {
            var context = await _ctxEnhancer.Build(ct);
            var result = await _client.Get(new UserGetRequest { Id = id }, context);
            return result.User?.MapFromReadModel();
        }

        public async Task<EntityPagination<User>> SearchForResourcesAsync(
            int offset,
            int limit,
            QueryParams? queryParams = null,
            IEnumerable<Guid>? ids = default,
            UserType? userType = default, 
            CancellationToken ct = default)
        {
            var context = await _ctxEnhancer.Build(ct);
            var result = await _client.Find(new UserSearchRequest
            {
                Offset = offset,
                Limit = limit,
                Search = queryParams?.Search,
                OrderBy = queryParams?.OrderBy ?? "Id",
                SortingOrder = queryParams == null ? SortingOrder.Ascending : (SortingOrder)queryParams.SortingOrder,
                Ids = ids?.ToArray(),
                Type = userType
            }, context);
            return new EntityPagination<User>(offset, limit, result.TotalItems, result.Users.MapFromReadModel());
        }
    }
}