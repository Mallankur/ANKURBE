using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Application.Extensions;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Application.Validators;
using Adform.Bloom.Runtime.Host.Controllers;
using Adform.Bloom.Runtime.Read.Entities;
using AutoFixture;
using FluentAssertions;
using FluentResults;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace Adform.Bloom.Runtime.Api.Test.Controllers
{
    public class RuntimeControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<IExistenceQueryValidator> _validator;
        private readonly RuntimeController _controller;
        private readonly Fixture _fixture;

        public RuntimeControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _validator = new Mock<IExistenceQueryValidator>();
            _controller = new RuntimeController(_mediator.Object, _validator.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task NodesExist_InvalidRequest_ReturnsBadRequest()
        {
            var query = _fixture.Create<NodeExistenceQuery>();
            var validationError = new ValidationResult(_fixture.CreateMany<ValidationFailure>());
            _validator.Setup(v => v.Validate(query))
                .Returns(validationError);

            var result = await _controller.NodesExist(query);

            result.Should().BeEquivalentTo(_controller.BadRequest(validationError.ToString()));
        }

        [Fact]
        public async Task NodesExist_CorrectRequest_ReturnsSuccessfulResult()
        {
            var query = _fixture.Create<NodeExistenceQuery>();
            var validationError = new ValidationResult();
            _validator.Setup(v => v.Validate(query))
                .Returns(validationError);
            var expectedResult = Result.Ok<bool>(true);
            _mediator.Setup(m => m.Send(query, CancellationToken.None)).ReturnsAsync(expectedResult);

            var result = await _controller.NodesExist(query);

            result.Should().BeEquivalentTo(_controller.Ok(new ExistenceResult { Exists = expectedResult.ThrowIfException() }));
        }

        [Fact]
        public async Task LegacyTenantsExist_InvalidRequest_ReturnsBadRequest()
        {
            var query = _fixture.Create<LegacyTenantExistenceQuery>();
            var validationError = new ValidationResult(_fixture.CreateMany<ValidationFailure>());
            _validator.Setup(v => v.Validate(query))
                .Returns(validationError);

            var result = await _controller.LegacyTenantsExist(query);

            result.Should().BeEquivalentTo(_controller.BadRequest(validationError.ToString()));
        }

        [Fact]
        public async Task LegacyTenantsExist_CorrectRequest_ReturnsSuccessfulResult()
        {
            var query = _fixture.Create<LegacyTenantExistenceQuery>();
            var validationError = new ValidationResult();
            _validator.Setup(v => v.Validate(query))
                .Returns(validationError);
            var expectedResult = Result.Ok<bool>(true);
            _mediator.Setup(m => m.Send(query, CancellationToken.None)).ReturnsAsync(expectedResult);

            var result = await _controller.LegacyTenantsExist(query);

            result.Should().BeEquivalentTo(_controller.Ok(new ExistenceResult { Exists = expectedResult.ThrowIfException() }));
        }

        [Fact]
        public async Task RoleExists_InvalidRequest_ReturnsBadRequest()
        {
            var query = _fixture.Create<RoleExistenceQuery>();
            var validationError = new ValidationResult(_fixture.CreateMany<ValidationFailure>());
            _validator.Setup(v => v.Validate(query))
                .Returns(validationError);

            var result = await _controller.RoleExists(query);

            result.Should().BeEquivalentTo(_controller.BadRequest(validationError.ToString()));
        }

        [Fact]
        public async Task RoleExist_CorrectRequest_ReturnsSuccessfulResult()
        {
            var query = _fixture.Create<RoleExistenceQuery>();
            var validationError = new ValidationResult();
            _validator.Setup(v => v.Validate(query))
                .Returns(validationError);
            var expectedResult = Result.Ok<bool>(true);
            _mediator.Setup(m => m.Send(query, CancellationToken.None)).ReturnsAsync(expectedResult);

            var result = await _controller.RoleExists(query);
            result.Should().BeEquivalentTo(_controller.Ok(new ExistenceResult {Exists = expectedResult.ThrowIfException()}));
        }

        [Fact]
        public async Task Intersect_InvalidRequest_ReturnsBadRequest()
        {
            var query = _fixture.Create<SubjectIntersectionQuery>();
            var validationError = new ValidationResult(_fixture.CreateMany<ValidationFailure>());
            _validator.Setup(v => v.Validate(query))
                .Returns(validationError);

            var result = await _controller.Intersect(query);

            result.Should().BeEquivalentTo(_controller.BadRequest(validationError.ToString()));
        }

        [Fact]
        public async Task Intersect_CorrectRequest_ReturnsSuccessfulResult()
        {
            var query = _fixture.Create<SubjectIntersectionQuery>();
            var validationError = new ValidationResult();
            _validator.Setup(v => v.Validate(query))
                .Returns(validationError);
            IEnumerable<RuntimeResult> expectedResult = new List<RuntimeResult>();
            _mediator.Setup(m => m.Send(query, CancellationToken.None)).ReturnsAsync(expectedResult);
            var result = await _controller.Intersect(query);

            result.Should().BeEquivalentTo(_controller.Ok(new List<RuntimeResult>()));
        }
    }
}