using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Abstractions.Persistence;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Infrastructure.Decorators;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Bogus;
using Moq;
using Xunit;

namespace Adform.Bloom.Read.Unit.Test.Infrastructure;

public class MeasuredBusinessAccountRepositoryTests
{
    private readonly Mock<ICustomHistogram> _histogramMock = new Mock<ICustomHistogram>();
    private readonly MeasuredRepository<BusinessAccount, BusinessAccountWithCount> _repo;
    private readonly Mock<IRepository<BusinessAccount, BusinessAccountWithCount>> _inner = new Mock<IRepository<BusinessAccount, BusinessAccountWithCount>>();
    private Faker<BusinessAccount> _faker;

    public MeasuredBusinessAccountRepositoryTests()
    {

        _repo =
            new MeasuredRepository<BusinessAccount, BusinessAccountWithCount> (_inner.Object, _histogramMock.Object);
        _faker = new Faker<BusinessAccount>()
            .RuleFor(o => o.Name, p => p.Company.CompanyName())
            .RuleFor(o => o.LegacyId, p => p.Random.Int())
            .RuleFor(o => o.Type, p => p.Random.Int(0,4))
            .RuleFor(o => o.Status, p => p.Random.Int(0, 5));
    }

    [Fact]
    public async Task FindByIdAsync_Invokes_Correct_Method()
    {
        var testUser = _faker.Generate();
        await _repo.FindByIdAsync(testUser.Id, CancellationToken.None);

        _inner.Verify(
            m => m.FindByIdAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Once);
        _histogramMock.Verify(m => m.Observe(It.IsAny<double>(), It.IsAny<string[]>()));
    }

    [Fact]
    public async Task FindAsync_Invokes_Correct_Method()
    {
        var testBusinessAccount = _faker.Generate();
        await _repo.FindAsync(0,
            100,
            "Id",
            SortingOrder.Ascending,
            testBusinessAccount.Name,
            new[] { testBusinessAccount.Id },
            null, 
            CancellationToken.None);

        _inner.Verify(
            m => m.FindAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<SortingOrder>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<int?>(),
                CancellationToken.None), Times.Once);
        _histogramMock.Verify(m => m.Observe(It.IsAny<double>(), It.IsAny<string[]>()));
    }
}