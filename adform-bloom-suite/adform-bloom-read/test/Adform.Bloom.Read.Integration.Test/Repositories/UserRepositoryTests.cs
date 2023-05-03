using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Read.Contracts.User;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Integration.Test.Collections;
using Adform.Bloom.Read.Integration.Test.Helpers;
using Adform.Bloom.Read.Integration.Test.Setup;
using Bogus;
using Xunit;
using SortingOrder = Adform.Bloom.Read.Domain.Entities.SortingOrder;

namespace Adform.Bloom.Read.Integration.Test.Repositories;

[Collection(nameof(RepositoriesCollection))]
public class UserRepositoryTests : IClassFixture<TestsFixture>
{
    private readonly TestsFixture _fixture;
    private readonly Faker<User> _faker;

    public UserRepositoryTests(TestsFixture fixture)
    {
        _fixture = fixture;
        _faker = fixture.UserFaker;
    }

    [Theory]
    [InlineData(Constants.Subject1, nameof(Constants.Subject1))]
    [InlineData(Constants.Subject2, nameof(Constants.Subject2))]
    [InlineData(Constants.Subject3, nameof(Constants.Subject3))]
    [InlineData(Constants.Subject4, nameof(Constants.Subject4))]
    [InlineData(Constants.Subject5, nameof(Constants.Subject5))]
    [InlineData(Constants.Subject6, nameof(Constants.Subject6))]
    [InlineData(Constants.Subject7, nameof(Constants.Subject7))]
    [InlineData(Constants.Subject8, nameof(Constants.Subject8))]
    [InlineData(Constants.Subject9, nameof(Constants.Subject9))]
    public async Task FindByIdAsync_Scenario(string userId, string userName)
    {
        // Arrange
        var id = Guid.Parse(userId);

        // Act
        var user = await _fixture.UserRepository.FindByIdAsync(id, CancellationToken.None);
        // Assert
        Assert.NotNull(user);
        Assert.Equal(id, user.Id);
        Assert.Equal(userName, user.Name);
        Assert.Equal(userName.ToLowerInvariant(), user.Username);
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
        UserWithCount[] result)
    {
        // Arrange
        if (!string.IsNullOrEmpty(search) || ids != null)
        {
            result = result.Select(c =>
            {
                c.TotalCount = 1;
                return c;
            }).ToArray();
        }

        // Act
        var users = await _fixture.UserRepository.FindAsync(
            offset,
            limit,
            orderBy,
            sortingOrder,
            search,
            ids,
            type,
            CancellationToken.None);

        // Assert
        Assert.Equal(result.Length, users.Count());
        Assert.True(users.OrderBy(o => o.Id).Select(o => o.Id)
            .SequenceEqual(result.OrderBy(o => o.Id).Select(o => o.Id)));
        Assert.Equal(result.First().TotalCount, users.First().TotalCount);
    }

    [Fact]
    public async Task AddAsync_RemoveAsync_Scenario()
    {
        // Arrange
        var testUser = _faker.Generate();

        // Act
        await _fixture.UserRepository.AddUserAsync(testUser);

        // Assert
        var user = await _fixture.UserRepository.FindByIdAsync(testUser.Id, CancellationToken.None);
        Assert.NotNull(user);
        await _fixture.UserRepository.RemoveUserAsync(testUser.Id);
        user = await _fixture.UserRepository.FindByIdAsync(testUser.Id, CancellationToken.None);
        Assert.Null(user);
    }

    #region Scenarios

    public static TheoryData<int, int, string, SortingOrder, string?, IEnumerable<Guid>?, int?, UserWithCount[]>
        FindScenarios()
    {
        var seed = new List<(Guid, string)>()
        {
            (Guid.Parse(Constants.Subject1), nameof(Constants.Subject1)),
            (Guid.Parse(Constants.Subject2), nameof(Constants.Subject2)),
            (Guid.Parse(Constants.Subject3), nameof(Constants.Subject3)),
            (Guid.Parse(Constants.Subject4), nameof(Constants.Subject4)),
            (Guid.Parse(Constants.Subject5), nameof(Constants.Subject5)),
            (Guid.Parse(Constants.Subject6), nameof(Constants.Subject6)),
            (Guid.Parse(Constants.Subject7), nameof(Constants.Subject7)),
            (Guid.Parse(Constants.Subject8), nameof(Constants.Subject8)),
            (Guid.Parse(Constants.Subject9), nameof(Constants.Subject9)),
            (Guid.Parse(Constants.Trafficker1), nameof(Constants.Trafficker1)),
            (Guid.Parse(Constants.Trafficker2), nameof(Constants.Trafficker2)),
            (Guid.Parse(Constants.Trafficker3), nameof(Constants.Trafficker3))
        };
        var users = seed.Select(MapToUserWithCount).ToArray();
        var data =
            new TheoryData<int, int, string, SortingOrder, string?, IEnumerable<Guid>?, int?, UserWithCount[]>();
        data.Add(0, 100, "Id", SortingOrder.Ascending, null, null, null, users);
        data.Add(0, 100, "NotExistant", SortingOrder.Ascending, null, null, null, users);
        data.Add(0, 3, "Name", SortingOrder.Ascending, null, null, null, users[..3]);
        data.Add(0, 3, "Name", SortingOrder.Descending, null, null, null, users[9..]);
        data.Add(0, 3, "LastName", SortingOrder.Descending, null, null, null, users[9..]);
        data.Add(0, 100, "Id", SortingOrder.Ascending, users[0].Name, null, null, users[..1]);
        data.Add(0, 100, "Id", SortingOrder.Ascending, users[0].Name.ToLowerInvariant(), null, null, users[..1]);
        data.Add(0, 100, "Id", SortingOrder.Ascending, users[0].FirstName.ToLowerInvariant(), null, null,
            users[..1]);
        data.Add(0, 100, "Id", SortingOrder.Ascending, users[0].LastName.ToLowerInvariant(), null, null,
            users[..1]);
        data.Add(0, 100, "Id", SortingOrder.Ascending, null, new Guid[] {users[0].Id}, null, users[..1]);
        var traffickers = seed.Skip(9).Select(MapToUserWithCount).ToArray();
        data.Add(0, 3, "Id", SortingOrder.Ascending, null, null, (int) UserType.Trafficker, traffickers.Select(c =>
        {
            c.TotalCount = 3;
            return c;
        }).ToArray());
        var masterAccounts = seed.Take(9).Select(MapToUserWithCount).ToArray();
        data.Add(0, 9, "Id", SortingOrder.Ascending, null, null, (int) UserType.MasterAccount, masterAccounts.Select(c =>
        {
            c.TotalCount = 9;
            return c;
        }).ToArray());

        return data;
    }

    private static UserWithCount MapToUserWithCount((Guid, string) input)
    {
        var id = input.Item1;
        var name = input.Item2;
        var index = Regex.Match(name, @"\d+").Value;

        return new UserWithCount()
        {
            Id = id,
            Email = $"{name.ToLowerInvariant()}@test",
            FirstName = $"Sub{index}",
            LastName = $"Last{index}",
            Name = name,
            Username = name.ToLowerInvariant(),
            TotalCount = 12
        };
    }

    #endregion
}