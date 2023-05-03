using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Bogus;
using Moq;
using ProtoBuf.Grpc;
using Xunit;

namespace Adform.Bloom.Unit.Test.DataAccess
{
    public class BusinessAccountReadModelProviderTests
    {
        private Mock<IBusinessAccountService> _client;
        private Mock<ICallContextEnhancer> _context;

        public BusinessAccountReadModelProviderTests()
        {
            _client = new Mock<IBusinessAccountService>();
            _context = new Mock<ICallContextEnhancer>();
        }

        [Fact]
        public async Task SearchForResources_For_Tenant_Type_Invokes_Proper_Methods()
        {
            // Arrange
            var ctx = new CallContext();
            var businessAccountFaker = new Faker<BusinessAccountResult>()
                .RuleFor(o => o.Name, f => f.Company.CompanyName())
                .RuleFor(o => o.Status, f => f.PickRandom<BusinessAccountStatus>())
                .RuleFor(o => o.Type, f => f.PickRandom<BusinessAccountType>());
            var businessAccounts = businessAccountFaker.Generate(3);
            _context.Setup(o => o.Build(It.IsAny<CancellationToken>())).ReturnsAsync(ctx);
            _client.Setup(
                    o => o.FindBusinessAccounts(It.IsAny<BusinessAccountSearchRequest>(), It.IsAny<CallContext>()))
                .ReturnsAsync(new BusinessAccountSearchResult
                {
                    BusinessAccounts = businessAccounts
                });
            var provider = new BusinessAccountReadModelProvider(_client.Object, _context.Object);
            var ids = businessAccounts.Select(o => o.Id);

            // Act
            var result = await provider.SearchForResourcesAsync(0, 100, new QueryParams(), ids);
            // Assert
            Assert.Equal(businessAccounts.Count, result.Data.Count());
            _client.Verify(
                o => o.FindBusinessAccounts(It.Is<BusinessAccountSearchRequest>(p => p.Ids.SequenceEqual(ids)),
                    It.Is<CallContext>(p => p.RequestHeaders == ctx.RequestHeaders)), Times.Once);
            _context.Verify(o => o.Build(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}