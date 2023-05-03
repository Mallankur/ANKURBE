using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Abstractions.Persistence;
using Adform.Bloom.Read.Application.Handlers;
using Adform.Bloom.Read.Application.Queries;
using Adform.Bloom.Read.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Adform.Bloom.Read.Unit.Test.Application;

public class BusinessAccountQueryHandlerTests
{
    private readonly BusinessAccountQueryHandler _handler;
    private readonly Mock<IRepository<BusinessAccount, BusinessAccountWithCount>> _businessAccountRepository;

    public BusinessAccountQueryHandlerTests()
    {
        _businessAccountRepository = new Mock<IRepository<BusinessAccount, BusinessAccountWithCount>>();
        _handler = new BusinessAccountQueryHandler(_businessAccountRepository.Object, NullLogger<BusinessAccountQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_Calls_FindAsync_And_Sends_Correct_Data()
    {
        //Arrange
        var searchTerm = Guid.NewGuid().ToString();
        var query = new SearchBusinessAccountQuery(0, 100, "Id", SortingOrder.Ascending, searchTerm, new List<Guid>(), 0);

        //Act
        var res = await _handler.Handle(query, CancellationToken.None);

        //Assert
        _businessAccountRepository.Verify(m => m.FindAsync(
            It.Is<int>(p => p == 0),
            It.Is<int>(p => p == 100),
            It.Is<string>(p => p == "Id"),
            It.Is<SortingOrder>(p => p == SortingOrder.Ascending),
            It.Is<string>(p => p == searchTerm),
            It.IsAny<IEnumerable<Guid>>(), It.IsAny<int?>(),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_Calls_GetAsync_And_Sends_Correct_Data()
    {
        //Arrange
        var id = Guid.NewGuid();
        var query = new GetBusinessAccountQuery(id);

        //Act
        var res = await _handler.Handle(query, CancellationToken.None);

        //Assert
        _businessAccountRepository.Verify(x => x.FindByIdAsync(id, CancellationToken.None));
    }


    [Theory]
    [InlineData(null, null)]
    public async Task HandlerValidatesRequest(string search, IEnumerable<Guid> ids)
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(new SearchBusinessAccountQuery(0, 100, "Id", SortingOrder.Ascending, search, ids, null), CancellationToken.None));
    }
}