using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts.User;
using Adform.Ciam.SharedKernel.Entities;

namespace Adform.Bloom.DataAccess.Providers.ReadModel
{
    public interface IUserReadModelProvider
    {
        Task<User?> SearchForResourceAsync(Guid id, CancellationToken ct = default);
        Task<EntityPagination<User>> SearchForResourcesAsync(
            int offset, 
            int limit,
            QueryParams? queryParams = null,
            IEnumerable<Guid>? ids = default, 
            UserType? userType = default, 
            CancellationToken ct = default);
    }
}