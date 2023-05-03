using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Application.Validators;
using Adform.Bloom.Runtime.Host.Graph.ExistenceCheck;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using AutoFixture;
using FluentResults;
using FluentValidation.Results;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Adform.Bloom.Runtime.Api.Test.Graph.Queries
{
    public class ExistenceEvaluationQueryTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<IExistenceQueryValidator> _validator;
        private readonly Fixture _fixture;

        public ExistenceEvaluationQueryTests()
        {
            _mediator = new Mock<IMediator>();
            _validator = new Mock<IExistenceQueryValidator>();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task NodesExist_InvalidRequest_ShouldThrowBadRequest()
        {
            // Arrange
            var query = _fixture.Create<NodeExistenceQuery>();
            var validationError = new ValidationResult(_fixture.CreateMany<ValidationFailure>());
            _validator.Setup(v => v.Validate(It.IsAny<NodeExistenceQuery>()))
                .Returns(validationError);

            var (provider, executor) = await GenerateExecutor(_mediator.Object, _validator.Object);

            var payload = @"
                query nodesExistCheckQuery($nodes: [NodeDescriptor]!) {
                  nodesExistCheck(nodes: $nodes) {
                    exists
                  }
                }";

            var request = QueryRequestBuilder.New()
                .SetQuery(payload)
                .SetServices(provider)
                .AddVariableValue("nodes", query.NodeDescriptors)
                .Create();

            // Act
            var action = await executor.ExecuteAsync(request);

            // Assert
            Assert.NotEmpty(action.Errors);
            Assert.True(action.Errors.Any(o => o.Exception.GetType() == typeof(BadRequestException)));
        }

        [Fact]
        public async Task NodesExist_CorrectRequest_ReturnsSuccessfulResult()
        {
            // Arrange
            var query = _fixture.Create<NodeExistenceQuery>();
            var validationError = new ValidationResult();
            _validator.Setup(v => v.Validate(It.IsAny<NodeExistenceQuery>()))
                .Returns(validationError);
            var expectedResult = true;
            _mediator.Setup(m => m.Send(It.IsAny<NodeExistenceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            var (provider, executor) = await GenerateExecutor(_mediator.Object, _validator.Object);

            var payload = @"
                query nodesExistCheckQuery($nodes: [NodeDescriptor]!) {
                  nodesExistCheck(nodes: $nodes) {
                    exists
                  }
                }";

            var request = QueryRequestBuilder.New()
                .SetQuery(payload)
                .SetServices(provider)
                .AddVariableValue("nodes", query.NodeDescriptors)
                .Create();

            // Act
            var action = await executor.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.Null(action.Errors);
            var json = action.ToJson();
        }

        [Fact]
        public async Task LegacyTenantsExistCheck_InvalidRequest_ShouldThrowBadRequest()
        {
            // Arrange
            var query = _fixture.Create<LegacyTenantExistenceQuery>();
            var validationError = new ValidationResult(_fixture.CreateMany<ValidationFailure>());
            _validator.Setup(v => v.Validate(It.IsAny<LegacyTenantExistenceQuery>()))
                .Returns(validationError);

            var (provider, executor) = await GenerateExecutor(_mediator.Object, _validator.Object);

            var payload = @"
                query legacyTenantsExistCheckQuery($tenantType: String!, $tenantLegacyIds: [Int!]!) {
                  legacyTenantsExistCheck(tenantType: $tenantType, tenantLegacyIds: $tenantLegacyIds) {
                    exists
                  }
                }";
            var request = QueryRequestBuilder.New()
                         .SetQuery(payload)
                         .SetServices(provider)
                         .AddVariableValue("tenantType", query.TenantType)
                         .AddVariableValue("tenantLegacyIds", query.TenantLegacyIds)
                         .Create();

            // Act
            var action = await executor.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotEmpty(action.Errors);
            Assert.True(action.Errors.Any(o => o.Exception.GetType() == typeof(BadRequestException)));
        }

        [Fact]
        public async Task LegacyTenantsExistCheck_CorrectRequest_ReturnsSuccessfulResult()
        {
            // Arrange
            var query = _fixture.Create<LegacyTenantExistenceQuery>();
            var validationError = new ValidationResult();
            _validator.Setup(v => v.Validate(It.IsAny<LegacyTenantExistenceQuery>()))
                .Returns(validationError);
            var expectedResult = Result.Ok<bool>(true);
            _mediator.Setup(m => m.Send(It.IsAny<LegacyTenantExistenceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
            
            var (provider, executor) = await GenerateExecutor(_mediator.Object, _validator.Object);

            var payload = @"
                query legacyTenantsExistCheckQuery($tenantType: String!, $tenantLegacyIds: [Int!]!) {
                  legacyTenantsExistCheck(tenantType: $tenantType, tenantLegacyIds: $tenantLegacyIds) {
                    exists
                  }
                }";
            var request = QueryRequestBuilder.New()
                         .SetQuery(payload)
                         .SetServices(provider)
                         .AddVariableValue("tenantType", query.TenantType)
                         .AddVariableValue("tenantLegacyIds", query.TenantLegacyIds)
                         .Create();

            // Act
            var action = await executor.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.Null(action.Errors);
            var json = action.ToJson();
        }

        [Fact]
        public async Task RoleExistsCheck_InvalidRequest_ShouldThrowBadRequest()
        {
            // Arrange
            var query = _fixture.Create<RoleExistenceQuery>();
            var validationError = new ValidationResult(_fixture.CreateMany<ValidationFailure>());
            _validator.Setup(v => v.Validate(It.IsAny<RoleExistenceQuery>()))
                .Returns(validationError);

            var (provider, executor) = await GenerateExecutor(_mediator.Object, _validator.Object);

            var payload = @"
                query roleExistsCheckQuery($tenantId: ID!, $roleName: String!) {
                  roleExistsCheck(tenantId: $tenantId, roleName:$roleName) {
                    exists
                  }
                }";

            var request = QueryRequestBuilder.New()
                         .SetQuery(payload)
                         .SetServices(provider)
                         .AddVariableValue("tenantId", query.TenantId.ToString())
                         .AddVariableValue("roleName", query.RoleName)
                         .Create();
            // Act
            var action = await executor.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.NotEmpty(action.Errors);
            Assert.True(action.Errors.Any(o => o.Exception.GetType() == typeof(BadRequestException)));
        }

        [Fact]
        public async Task RoleExistsCheck_CorrectRequest_ReturnsSuccessfulResult()
        {
            // Arrange
            var query = _fixture.Create<RoleExistenceQuery>();
            var validationError = new ValidationResult();
            _validator.Setup(v => v.Validate(It.IsAny<RoleExistenceQuery>()))
                .Returns(validationError);
            var expectedResult = Result.Ok<bool>(true);
            _mediator.Setup(m => m.Send(It.IsAny<RoleExistenceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            var (provider, executor) = await GenerateExecutor(_mediator.Object, _validator.Object);

            var payload = @"
                query roleExistsCheckQuery($tenantId: ID!, $roleName: String!) {
                  roleExistsCheck(tenantId: $tenantId, roleName:$roleName) {
                    exists
                  }
                }";
            var request = QueryRequestBuilder.New()
                                     .SetQuery(payload)
                                     .SetServices(provider)
                                     .AddVariableValue("tenantId", query.TenantId.ToString())
                                     .AddVariableValue("roleName", query.RoleName)
                                     .Create();

            // Act
            var action = await executor.ExecuteAsync(request, CancellationToken.None);

            // Assert
            Assert.Null(action.Errors);
        }

        private async Task<(ServiceProvider Provider, IRequestExecutor Executor)> GenerateExecutor(IMediator mediator, IExistenceQueryValidator validator)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IExistenceQueryValidator>(validator);
            services.AddSingleton<IMediator>(mediator);

            var executor = await services
                .AddGraphQL().AddType(new UuidType('D'))
                .AddMaxExecutionDepthRule(100)
                .AddQueryType(p => p.Name(OperationTypeNames.Query))
                .AddTypeExtension<ExistenceCheckGQLQuery>()
                .AddType<ExistenceResultType>()
                .ModifyRequestOptions(c =>
                {
                    c.Complexity.MaximumAllowed = 100;
                })
                .BuildRequestExecutorAsync();

            var provider = services.BuildServiceProvider();

            return (provider, executor);
        }
    }

}