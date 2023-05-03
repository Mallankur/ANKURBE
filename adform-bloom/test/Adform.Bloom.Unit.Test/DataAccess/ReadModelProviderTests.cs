using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts.User;
using Adform.Ciam.OngDb.Repository;
using Bogus;
using Moq;
using ProtoBuf.Grpc;
using Xunit;

namespace Adform.Bloom.Unit.Test.DataAccess
{
    public class UserReadModelProviderTests
    {
        private Mock<IUserService> _client;
        private Mock<ICallContextEnhancer> _context;

        public UserReadModelProviderTests()
        {
            _client = new Mock<IUserService>();
            _context = new Mock<ICallContextEnhancer>();
        }

        [Fact]
        public async Task SearchForResources_For_Subject_Type_Invokes_Proper_Methods()
        {
            // Arrange
            var search = Guid.NewGuid().ToString();
            var ctx = new CallContext();
            var usersFaker = new Faker<UserResult>()
                .RuleFor(o => o.Name, f => f.Person.FullName)
                .RuleFor(o => o.Email, f => f.Person.Email)
                .RuleFor(o => o.Username, f => f.Person.UserName)
                .RuleFor(o => o.Locale, f => f.Locale)
                .RuleFor(o => o.Phone, f => f.Person.Phone);
            var users = usersFaker.Generate(3);
            _context.Setup(o => o.Build(It.IsAny<CancellationToken>())).ReturnsAsync(ctx);
            _client.Setup(o => o.Find(It.IsAny<UserSearchRequest>(), It.IsAny<CallContext>()))
                .ReturnsAsync(new UserSearchResult()
                {
                    Users = users
                });
            var provider = new UserReadModelProvider(_client.Object, _context.Object);
            var queryParams = new QueryParams()
            {
                OrderBy = "Id",
                Search = search,
                SortingOrder = SortingOrder.Ascending
            };

            // Act
            var result = await provider.SearchForResourcesAsync(0,100, queryParams, null);
            // Assert
            Assert.Equal(users.Count, result.Data.Count());
            _client.Verify(o => o.Find(It.Is<UserSearchRequest>(p => p.Search == search), It.Is<CallContext>(p => p.RequestHeaders == ctx.RequestHeaders)), Times.Once);
            _context.Verify(o => o.Build(It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}