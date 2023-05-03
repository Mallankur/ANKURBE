using Adform.Bloom.DataAccess.Repositories;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Extensions;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.OngDb.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Mediatr.Extensions;
using Adform.Bloom.Write.Extensions;
using Neo4jClient.Transactions;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.Transactions
{
    [Collection(nameof(TransactionCollection))]
    public class TransactionsTests : IClassFixture<TestsFixture>
    {
        public TransactionsTests(TestsFixture fixture)
        {
            _fixture = fixture;

            _services = new ServiceCollection();
            _services.AddMediatR(typeof(AssignPermissionToRoleCommandHandler).Assembly);
            _services.ConfigureQueries();
            _services.ConfigureCommands(_fixture.Configuration);
            _services.ConfigureNeo(_fixture.Configuration);
            _services.AddSingleton<IAdminGraphRepository, AdminGraphRepository>();
            _services.RegisterTransactionalBehaviors("CommandHandler");


            _prv = _services.BuildServiceProvider(true);
        }

        private readonly ServiceCollection _services;
        private readonly ServiceProvider _prv;

        private readonly TestsFixture _fixture;

        [Fact]
        [Order(0)]
        public async Task Make_Changes_And_Commit()
        {
            // Arrange
            var numberOfPoliciesBefore = 0L;
            var rep = _prv.GetService<IAdminGraphRepository>();

            numberOfPoliciesBefore = await rep.GetCountAsync<Policy>(p => true);

            Policy parentPolicy = null;
            Policy childPolicy = null;

            // Act
            using (var scope = _prv.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                using var uow = scope.ServiceProvider.GetService<ITransactionalGraphClient>().BeginTransaction();

                parentPolicy = await mediator.Send(new CreatePolicyCommand(new ClaimsPrincipal(), null, "Parent"));
                childPolicy =
                    await mediator.Send(new CreatePolicyCommand(new ClaimsPrincipal(), parentPolicy.Id, "Child"));
                await uow.CommitAsync();
            }

            // Assert
            var numberOfPoliciesAfter = await rep.GetCountAsync<Policy>(p => true);
            var hasLink = await rep.HasRelationshipAsync<Policy, Policy>(
                c => c.Id == childPolicy.Id,
                p => p.Id == parentPolicy.Id,
                Constants.ChildOfLink);

            Assert.Equal(numberOfPoliciesBefore + 2, numberOfPoliciesAfter);
            Assert.True(hasLink);
        }

        [Fact]
        [Order(1)]
        public async Task Make_Changes_And_Rollback()
        {
            // Arrange
            var numberOfPoliciesBefore = 0L;
            var rep = _prv.GetService<IAdminGraphRepository>();

            numberOfPoliciesBefore = await rep.GetCountAsync<Policy>(p => true);

            Policy parentPolicy = null;
            Policy childPolicy = null;

            // Act
            using (var scope = _prv.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                using var uow = scope.ServiceProvider.GetService<ITransactionalGraphClient>().BeginTransaction();

                parentPolicy = await mediator.Send(new CreatePolicyCommand(new ClaimsPrincipal(), null, "Parent"));
                childPolicy =
                    await mediator.Send(new CreatePolicyCommand(new ClaimsPrincipal(), parentPolicy.Id, "Child"));
                await uow.RollbackAsync();
            }

            // Assert
            var numberOfPoliciesAfter = await rep.GetCountAsync<Policy>(p => true);
            var hasLink = await rep.HasRelationshipAsync<Policy, Policy>(
                c => c.Id == childPolicy.Id,
                p => p.Id == parentPolicy.Id,
                Constants.ChildOfLink);

            Assert.Equal(numberOfPoliciesBefore, numberOfPoliciesAfter);
            Assert.False(hasLink);
        }
    }
}