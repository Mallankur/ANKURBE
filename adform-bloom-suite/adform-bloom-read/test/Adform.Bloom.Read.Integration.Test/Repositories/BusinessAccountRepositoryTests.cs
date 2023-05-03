using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Integration.Test.Collections;
using Adform.Bloom.Read.Integration.Test.Helpers;
using Adform.Bloom.Read.Integration.Test.Setup;
using Bogus;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Read.Integration.Test.Repositories;

[Collection(nameof(RepositoriesCollection))]
public class BusinessAccountRepositoryTests : IClassFixture<TestsFixture>
{
    private readonly Faker<BusinessAccount> _faker;
    private readonly TestsFixture _fixture;

    public BusinessAccountRepositoryTests(TestsFixture fixture)
    {
        _fixture = fixture;
        _faker = fixture.BusinessAccountFaker;
    }

    [Theory]
    [InlineData(Constants.Tenant0, nameof(Constants.Tenant0),0)]
    [InlineData(Constants.Tenant1, nameof(Constants.Tenant1),1)]
    [InlineData(Constants.Tenant2, nameof(Constants.Tenant2),2)]
    [InlineData(Constants.Tenant3, nameof(Constants.Tenant3),3)]
    [InlineData(Constants.Tenant4, nameof(Constants.Tenant4),4)]
    [InlineData(Constants.Tenant5, nameof(Constants.Tenant5),5)]
    [InlineData(Constants.Tenant6, nameof(Constants.Tenant6),6)]
    [InlineData(Constants.Tenant7, nameof(Constants.Tenant7),7)]
    [InlineData(Constants.Tenant8, nameof(Constants.Tenant8),8)]
    [InlineData(Constants.Tenant9, nameof(Constants.Tenant9),9)]
    [InlineData(Constants.Tenant10, nameof(Constants.Tenant10),10)]
    [InlineData(Constants.Tenant11, nameof(Constants.Tenant11),11)]
    [InlineData(Constants.Tenant12, nameof(Constants.Tenant12),12)]
    [InlineData(Constants.Tenant13, nameof(Constants.Tenant13),13)]
    [InlineData(Constants.Tenant14, nameof(Constants.Tenant14),14)]
    public async Task FindByIdAsync_Scenario(string businessAccountId, string businessAccountName, int legacyId)
    {
        // Arrange
        var id = Guid.Parse(businessAccountId);

        // Act
        var businessAccount = await _fixture.BusinessAccountRepository.FindByIdAsync(id, CancellationToken.None);
        // Assert
        Assert.NotNull(businessAccount);
        Assert.Equal(id, businessAccount.Id);
        Assert.Equal(businessAccountName, businessAccount.Name);
        Assert.Equal(legacyId, businessAccount.LegacyId);
    }

    [Theory]
    [MemberData(nameof(FindScenarios))]
    public async Task FindAsync_Scenarios(int offset,
        int limit,
        string orderBy,
        SortingOrder sortingOrder,
        string? search,
        IEnumerable<Guid>? ids,
        int? type,
        BusinessAccountWithCount[] result)
    {
        // Arrange
        result = result.Select(c =>
        {
            c.TotalCount = (!string.IsNullOrEmpty(search) || ids != null || type != null) ? 1 : 15;
            return c;
        }).ToArray();

        // Act
        var businessAccounts = await _fixture.BusinessAccountRepository.FindAsync(
            offset,
            limit,
            orderBy,
            sortingOrder,
            search,
            ids,
            type,
            CancellationToken.None);

        // Assert
        Assert.Equal(result.Length, businessAccounts.Count());
        Assert.True(businessAccounts.OrderBy(o => o.Id).Select(o => o.Id)
            .SequenceEqual(result.OrderBy(o => o.Id).Select(o => o.Id)));
        Assert.True(businessAccounts.OrderBy(o => o.LegacyId).Select(o => o.LegacyId)
            .SequenceEqual(result.OrderBy(o => o.LegacyId).Select(o => o.LegacyId)));
        Assert.Equal(result.First().TotalCount, businessAccounts.First().TotalCount);
    }

    [Fact]
    public async Task AddAsync_RemoveAsync_Scenario()
    {
        // Arrange
        var testBusinessAccount = _faker.Generate();

        // Act
        await _fixture.BusinessAccountRepository.AddBusinessAccountAsync(testBusinessAccount);

        // Assert
        var businessAccount =
            await _fixture.BusinessAccountRepository.FindByIdAsync(testBusinessAccount.Id, CancellationToken.None);
        Assert.NotNull(businessAccount);
        await _fixture.BusinessAccountRepository.RemoveBusinessAccountAsync(testBusinessAccount.Id, businessAccount.Type);
        businessAccount =
            await _fixture.BusinessAccountRepository.FindByIdAsync(testBusinessAccount.Id, CancellationToken.None);
        Assert.Null(businessAccount);
    }

    #region Scenarios

    public static TheoryData<int, int, string, SortingOrder, string?, IEnumerable<Guid>?, int?, BusinessAccountWithCount[]>
        FindScenarios()
    {
        var seed = new List<(Guid, string, int)>()
        {
            (Guid.Parse(Constants.Tenant0), nameof(Constants.Tenant0),0),
            (Guid.Parse(Constants.Tenant1), nameof(Constants.Tenant1),1),
            (Guid.Parse(Constants.Tenant2), nameof(Constants.Tenant2),2),
            (Guid.Parse(Constants.Tenant3), nameof(Constants.Tenant3),3),
            (Guid.Parse(Constants.Tenant4), nameof(Constants.Tenant4),4),
            (Guid.Parse(Constants.Tenant5), nameof(Constants.Tenant5),5),
            (Guid.Parse(Constants.Tenant6), nameof(Constants.Tenant6),6),
            (Guid.Parse(Constants.Tenant7), nameof(Constants.Tenant7),7),
            (Guid.Parse(Constants.Tenant8), nameof(Constants.Tenant8),8),
            (Guid.Parse(Constants.Tenant9), nameof(Constants.Tenant9),9),
            (Guid.Parse(Constants.Tenant10), nameof(Constants.Tenant10),10),
            (Guid.Parse(Constants.Tenant11), nameof(Constants.Tenant11),11),
            (Guid.Parse(Constants.Tenant12), nameof(Constants.Tenant12),12),
            (Guid.Parse(Constants.Tenant13), nameof(Constants.Tenant13),13),
            (Guid.Parse(Constants.Tenant14), nameof(Constants.Tenant14),14)
        };
        var businessAccount = seed.Select(MapToBusinessAccountWithCount).ToArray();
        var data = new TheoryData<int, int, string, SortingOrder, string?, IEnumerable<Guid>?, int?, BusinessAccountWithCount[]>();
            
        data.Add(0, 100, "Id", SortingOrder.Ascending, null, null, null, businessAccount);
        data.Add(0, 100, "NotExistant", SortingOrder.Ascending, null, null, null, businessAccount);
        data.Add(0, 100, "Id", SortingOrder.Ascending, businessAccount[0].Name, null, null, businessAccount[..1]);
        data.Add(0, 100, "Id", SortingOrder.Ascending, businessAccount[0].Name.ToLowerInvariant(), null, null, businessAccount[..1]);
        data.Add(0, 100, "Id", SortingOrder.Ascending, null, new Guid[] { businessAccount[0].Id }, null, businessAccount[..1]);
        var ascBusinessAccount = new List<BusinessAccountWithCount>();
        ascBusinessAccount.AddRange(businessAccount[..2]);
        ascBusinessAccount.Add(businessAccount[10]);
        data.Add(0, 3, "Name", SortingOrder.Ascending, null, null, null, ascBusinessAccount.ToArray());
        var descBusinessAccount = new List<BusinessAccountWithCount>();
        descBusinessAccount.Add(businessAccount[9]);
        descBusinessAccount.Add(businessAccount[8]);
        descBusinessAccount.Add(businessAccount[7]);
        data.Add(0, 3, "Name", SortingOrder.Descending, null, null, null, descBusinessAccount.ToArray());
        data.Add(0, 100, "Id", SortingOrder.Ascending, null, null, 3, new[] { businessAccount[2] });
        return data;
    }

    private static BusinessAccountWithCount MapToBusinessAccountWithCount((Guid, string, int) input)
    {
        var id = input.Item1;
        var name = input.Item2;
        var legacyId= input.Item3;
        var index = Regex.Match(name, @"\d+").Value;

        return new BusinessAccountWithCount
        {
            Id = id,
            Name = name,
            LegacyId = legacyId,
            TotalCount = 15
        };
    }

    #endregion
}