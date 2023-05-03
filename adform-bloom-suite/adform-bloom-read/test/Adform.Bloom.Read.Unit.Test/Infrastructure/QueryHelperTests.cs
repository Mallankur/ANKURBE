using System;
using System.Collections.Generic;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Infrastructure.Helpers;
using Xunit;

namespace Adform.Bloom.Read.Unit.Test.Infrastructure;

public class QueryHelperTests
{
    [Theory]
    [MemberData(nameof(GetIns))]
    public void In_Returns_Expected_Result(IEnumerable<Guid> ids, string expectedResult)
    {
        var where = QueryHelper.GenerateInClause(ids);
        Assert.Equal(expectedResult, where);
    }

    [Fact]
    public void Select_User_Returns_Expected_Result()
    {
        var select = QueryHelper.GenerateSelect<User>();
        Assert.Equal("id, username, email, name, phone, timezone, locale, " +
                     "two_factor_required TwoFaEnabled, " +
                     "not notifications_disabled SecurityNotifications, " +
                     "coalesce(status, 0) status, type, first_name FirstName, " +
                     "last_name LastName, company, title", select);
    }

    [Fact]
    public void Select_BusinessAccount_Returns_Expected_Result()
    {
        var select = QueryHelper.GenerateSelect<BusinessAccount>();
        Assert.Equal(
            "id, legacy_id LegacyId, name, type, status",
            select);
    }
        
    [Theory]
    [MemberData(nameof(GetSortingString))]
    public void GenerateSorting_Returns_Expected_Result(string property, SortingOrder order, string output)
    {
        // Act
        var result = QueryHelper.GenerateSorting<User>(property, order);
        // Assert
        Assert.Equal(output, result);
    }

    public static TheoryData<string, SortingOrder, string> GetSortingString()
    {
        var data = new TheoryData<string, SortingOrder, string>();
        data.Add("Id", SortingOrder.Ascending, "Id ASC");
        data.Add("NotExistant", SortingOrder.Ascending, "Id ASC");
        data.Add("Username", SortingOrder.Ascending, "Username ASC, Id ASC");
        data.Add("LastName", SortingOrder.Ascending, "LastName ASC, Id ASC");
        return data;
    }

    public static TheoryData<IEnumerable<Guid>, string> GetIns()
    {
        var data = new TheoryData<IEnumerable<Guid>, string>();
        var uuid = Guid.NewGuid();
        data.Add(new[] {uuid}, $"id in ('{uuid}')");
        var arr = new[] {Guid.NewGuid(), Guid.NewGuid()};
        data.Add(arr, $"id in ('{arr[0]}','{arr[1]}')");
        arr = new Guid[0];
        data.Add(arr, "id in ('')");

        return data;
    }
}