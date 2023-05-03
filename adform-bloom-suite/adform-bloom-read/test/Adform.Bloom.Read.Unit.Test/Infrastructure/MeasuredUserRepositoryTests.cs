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
using User = Adform.Bloom.Read.Domain.Entities.User;

namespace Adform.Bloom.Read.Unit.Test.Infrastructure;

public class MeasuredUserRepositoryTests
{
    private readonly Mock<ICustomHistogram> _histogramMock = new Mock<ICustomHistogram>();
    private readonly Mock<IRepository<User, UserWithCount>> _inner = new Mock<IRepository<User, UserWithCount>>();
    private readonly MeasuredRepository<User, UserWithCount> _repo;
    private readonly Faker<User> _faker;

    public MeasuredUserRepositoryTests()
    {
        _repo = new MeasuredRepository<User, UserWithCount>(_inner.Object, _histogramMock.Object);
        _faker = new Faker<User>()
            .RuleFor(o => o.Name, p => p.Person.FirstName)
            .RuleFor(o => o.Username, p => p.Person.UserName)
            .RuleFor(o => o.Phone, p => p.Person.Phone)
            .RuleFor(o => o.Locale, p => p.Locale)
            .RuleFor(o => o.Email, p => p.Person.Email);
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
        var testUser = _faker.Generate();
        await _repo.FindAsync(0,
            100,
            "Id",
            SortingOrder.Ascending,
            testUser.Name,
            new[] { testUser.Id }, 
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