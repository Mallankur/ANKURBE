using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Abstractions.Persistence;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Infrastructure.Helpers;
using Dapper;

namespace Adform.Bloom.Read.Infrastructure.Repository;

public class BusinessAccountRepository : IRepository<BusinessAccount,BusinessAccountWithCount>
{
    public IDbConnection Connection { get; }

    private static readonly string Select = QueryHelper.GenerateSelect<BusinessAccount>();

    public BusinessAccountRepository(IDbConnection connection)
    {
        Connection = connection;
    }

    public Task<BusinessAccount?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Connection.QueryFirstOrDefaultAsync<BusinessAccount>(new CommandDefinition(
            $"SELECT {Select} FROM business_accounts WHERE id = @Id",
            new { Id = id }, cancellationToken: cancellationToken))!;
    }

    public Task<IEnumerable<BusinessAccountWithCount>> FindAsync(int offset, int limit, string orderBy, SortingOrder sortingOrder, string? search, IEnumerable<Guid>? ids,
        int? type, CancellationToken cancellationToken)
    {
        var inArray = string.Empty;
        var and = string.Empty;

        if (ids != null)
        {
            inArray = QueryHelper.GenerateInClause(ids);
        }

        if (!string.IsNullOrEmpty(search) && search.Length > 2)
        {
            var optionalAnd = inArray.Length > 0 ? " AND " : string.Empty;
            and = $"{optionalAnd}(name ~* @Search)";
        }

        if (type.HasValue)
        {
            var optionalAnd = and.Length > 0 || ids != null ? " AND " : string.Empty;
            and += $"{optionalAnd}type = @Type";
        }

        var where = inArray.Length > 0 || and.Length > 0 ? $"WHERE {inArray}{and}" : string.Empty;

        var orderAndSort = QueryHelper.GenerateSorting<BusinessAccount>(orderBy, sortingOrder);

        var command = new CommandDefinition(
            $"WITH pre AS (SELECT {Select} FROM business_accounts " +
            $"{where}) " +
            "SELECT * FROM (TABLE pre " +
            $"ORDER BY {orderAndSort} " +
            "LIMIT @Limit " +
            "OFFSET @Offset) sub " +
            "RIGHT JOIN(SELECT count(*) FROM pre) c(TotalCount) ON true;",
            new
            {
                Offset = offset,
                Limit = limit,
                Search = search,
                Type = type
            }, cancellationToken: cancellationToken);

        return Connection.QueryAsync<BusinessAccountWithCount>(command);
    }

}