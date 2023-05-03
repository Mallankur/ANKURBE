using System;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Abstractions.Persistence;
using Adform.Bloom.Read.Contracts.User;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Integration.Test.Collections;
using Adform.Bloom.Read.Integration.Test.Helpers;
using Adform.Bloom.Read.Integration.Test.Setup;
using Bogus;
using ProtoBuf.Grpc;
using Xunit;

namespace Adform.Bloom.Read.Integration.Test.Services;

[Collection(nameof(GrpcCollection))]
public class UserServiceTests : IClassFixture<TestsFixture>
{
    private readonly IUserService _userClient;
    private readonly Faker<User> _userFaker;
    private readonly IRepository<User, UserWithCount> _userRepository;

    public UserServiceTests(TestsFixture fixture)
    {
        _userClient = fixture.UserClient;
        _userFaker = fixture.UserFaker;
        _userRepository = fixture.UserRepository;
    }

    [Fact]
    public async Task Grpc_Find_Returns_Users()
    {
        // Arrange
        var testUser = _userFaker.Generate();
        await _userRepository.AddUserAsync(testUser);
        var testUser2 = _userFaker.Generate();
        await _userRepository.AddUserAsync(testUser2);

        // Act
        var users = await _userClient.Find(new UserSearchRequest
        {
            Search = testUser.Name[..6],
            Type = (UserType) testUser.Type
        }, new CallContext());

        // Assert
        Assert.Equal(testUser.Id, users.Users.First().Id);
    }

    [Fact]
    public async Task Grpc_Get_Returns_Users()
    {
        // Arrange
        var testUser = _userFaker.Generate();
        await _userRepository.AddUserAsync(testUser);

        // Act
        try
        {
            var user = await _userClient.Get(new UserGetRequest
            {
                Id = testUser.Id
            }, new CallContext());

            // Assert
            Assert.Equal(testUser.Id, user.User.Id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

}