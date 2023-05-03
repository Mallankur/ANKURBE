using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts.User;
using Adform.Ciam.OngDb.Core.Extensions;
using Adform.Ciam.OngDb.Extensions;
using Adform.Ciam.OngDb.Repository;
using Adform.Ciam.SharedKernel.Entities;
using Neo4jClient.Transactions;
using Subject = Adform.Bloom.Domain.Entities.Subject;
using Tenant = Adform.Bloom.Domain.Entities.Tenant;

namespace Adform.Bloom.DataAccess.Providers.Access
{
    public class UserByBusinessAccountAccessProvider : GraphRepository,
        IAccessProvider<BusinessAccount, QueryParamsTenantIds, User>
    {
        private readonly IUserReadModelProvider _readModelProvider;

        private static readonly string TenantToSubject =
            $"{Constants.BelongsIncomingLink.ToCypher()}(g:Group){Constants.MemberOfIncomingLink.ToCypher()}";

        public UserByBusinessAccountAccessProvider(ITransactionalGraphClient graphClient, 
            IUserReadModelProvider readModelProvider) : base(graphClient)
        {
            _readModelProvider = readModelProvider;
        }

        public async Task<EntityPagination<User>> EvaluateAccessAsync(ClaimsPrincipal subject,
            BusinessAccount context, int skip, int limit, QueryParamsTenantIds filter, CancellationToken cancellationToken = default)
        {
            const string subjectVariable = "s";
            var tenants = subject.GetTenants(limitTo: new[] {context.Id});

            var match =
                $"(t:{nameof(Tenant)}){TenantToSubject}({subjectVariable}:{nameof(Subject)})";
            var where = "t.Id in {tenants}";
            var andWhere = "true";

            var cypher = (await GraphClient).Cypher
                .Match(match)
                .Where(where)
                .AndWhere(andWhere)
                .With("s, 0 as c")
                .ReturnDistinct((s, c) => new CypherPaginationResult<Contracts.Output.Subject>
                    {Node = s.As<Contracts.Output.Subject?>(), TotalCount = c.As<int>()})
                .OrderByDual("s")
                .Skip(0)
                .Limit(int.MaxValue)
                .UnionAll()
                .Match(match)
                .Where(where)
                .AndWhere(andWhere)
                .WithParams(new Dictionary<string, object>
                {
                    {"tenants", tenants.Select(x => x.ToString())}
                })
                .With("null as s, count(distinct s) as c")
                .ReturnDistinct((s, c) => new CypherPaginationResult<Contracts.Output.Subject>
                    {Node = s.As<Contracts.Output.Subject?>(), TotalCount = c.As<int>()});
            
            var ids = (await cypher.ResultsAsync).ToEntityPagination(skip, limit);
            if (!(filter?.Search?.Length >= 3) && filter != null)
            {
                filter.Search = null;
            }

            if (ids.Data.Count < 1 || limit == 0)
            {
                return new EntityPagination<User>(ids.Offset, ids.Limit, ids.TotalItems,
                    new List<User>(0));
            }

            return await _readModelProvider.SearchForResourcesAsync(
                skip,
                limit,
                filter,
                ids.Data.Select(x => x.Id),
                !subject.IsAdformAdmin() ? UserType.MasterAccount : null,
                cancellationToken);
        }
    }
}