using System;
using System.Collections.Generic;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Infrastructure.Extensions;

namespace Adform.Bloom.Read.Infrastructure.Helpers;

public static class QueryHelper
{
    // building parameter manually because of Dapper-Psql issue:
    // https://www.gitmemory.com/issue/StackExchange/Dapper/315/491737982
    public static string GenerateInClause(IEnumerable<Guid> ids) =>
        $"id in ('{string.Join("','", ids)}')";

    public static string GenerateSelect<T>() =>
        typeof(T) switch
        {
            var t when t == typeof(User) => "id, username, email, name, phone, timezone, locale, " +
                                            $"two_factor_required {nameof(User.TwoFaEnabled)}, " +
                                            $"not notifications_disabled {nameof(User.SecurityNotifications)}, " +
                                            $"coalesce(status, 0) status, type, first_name {nameof(User.FirstName)}, " +
                                            $"last_name {nameof(User.LastName)}, company, title",
                
            var t when t == typeof(BusinessAccount) =>
                $"id, legacy_id {nameof(BusinessAccount.LegacyId)}, " +
                "name, type, status",
            _ => throw new NotSupportedException()
        };
        
    public static string GenerateSorting<T>(string str, SortingOrder sort = SortingOrder.Ascending)
    {
        var sortingOrder = sort == SortingOrder.Ascending ? "ASC" : "DESC";
        var property =  str.ToPropertyOrDefault<T>();
        var sorting = $"{property} {sortingOrder}";
        if(property != "Id")
            sorting += $", Id {sortingOrder}";
        return sorting;
    }
}