using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Abstractions.Persistence;
using Adform.Bloom.Read.Contracts;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Integration.Test.Collections;
using Adform.Bloom.Read.Integration.Test.Helpers;
using Adform.Bloom.Read.Integration.Test.Setup;
using Bogus;
using ProtoBuf.Grpc;
using Xunit;

namespace Adform.Bloom.Read.Integration.Test.Services;

[Collection(nameof(GrpcCollection))]
public class BusinessAccountServiceTests : IClassFixture<TestsFixture>
{
    private readonly Faker<BusinessAccount> _businessAccountFaker;
    private readonly IRepository<BusinessAccount, BusinessAccountWithCount> _businessAccountRepository;
    private readonly IBusinessAccountService _businessAccountClient;

    public BusinessAccountServiceTests(TestsFixture fixture)
    {
        _businessAccountClient = fixture.BusinessAccountClient;
        _businessAccountFaker = fixture.BusinessAccountFaker;
        _businessAccountRepository = fixture.BusinessAccountRepository;
    }

    [Fact]
    public async Task Grpc_Find_Returns_BusinessAccounts()
    {
        // Arrange
        var testBa = _businessAccountFaker.Generate();
        await _businessAccountRepository.AddBusinessAccountAsync(testBa);
        var testBa2 = _businessAccountFaker.Generate();
        await _businessAccountRepository.AddBusinessAccountAsync(testBa2);

        // Act
        var basById = await _businessAccountClient.FindBusinessAccounts(new BusinessAccountSearchRequest
        {
            Ids = new List<Guid> {testBa.Id},
            Type = (BusinessAccountType) testBa.Type
        }, new CallContext());
        var basByName = await _businessAccountClient.FindBusinessAccounts(new BusinessAccountSearchRequest
        {
            Search = testBa.Name[..6],
            Type = (BusinessAccountType) testBa.Type
        }, new CallContext());

        // Assert
        Assert.Equal(testBa.Id, basById.BusinessAccounts.First().Id);
        Assert.Equal(testBa.Id, basByName.BusinessAccounts.First().Id);
    }

    [Fact]
    public async Task Grpc_Get_Returns_BusinessAccount()
    {
        // Arrange
        var testBa = _businessAccountFaker.Generate();
        await _businessAccountRepository.AddBusinessAccountAsync(testBa);

        // Act
        var ba = await _businessAccountClient.GetBusinessAccount(new GetRequest
        {
            Id = testBa.Id
        }, CallContext.Default);

        // Assert
        Assert.Equal(testBa.Id, ba.BusinessAccount.Id);
    }
}