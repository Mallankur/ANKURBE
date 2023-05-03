using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Adform.AspNetCore.Paging;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.Swagger.Attributes;
using Adform.Ciam.Swagger.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Filters;
using LinkOperation = Adform.Bloom.Write.LinkOperation;

namespace Adform.Bloom.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class FeaturesController : DocumentedControllerBase
    {
        private readonly IMediator _mediator;

        public FeaturesController(
            IMediator mediator)
        {
            _mediator = mediator;
        }


        #region Query

        [HttpGet("{featureId}")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(Feature), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromRoute] Guid featureId, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new FeatureQuery(User, featureId), cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(IEnumerable<Feature>), StatusCodes.Status200OK)]
        [ProducesResponseHeaders(Headers.Pagination, StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] Page page, CancellationToken cancellationToken = default)
        {
            var result =
                await _mediator.Send(new FeaturesQuery(User, new QueryParamsTenantIdsInput(), page.Offset, page.Limit),
                    cancellationToken);
            return new OkWithPaginationResult<IEnumerable<Feature>>(result.Data, page,
                new Order {OrderBy = Constants.Parameters.Id, OrderDirection = OrderDirection.Descending},
                page.ReturnTotalCount ? result.TotalItems : (long?) null);
        }

        #endregion


        #region Mutation

        [HttpPost]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(Feature), StatusCodes.Status201Created)]
        [ProducesResponseHeaders(Headers.Limit, StatusCodes.Status201Created)]
        [SwaggerResponseHeader(StatusCodes.Status201Created, nameof(HeaderNames.Location), "string",
            "Location of resource")]
        public async Task<IActionResult> Create([FromBody] CreateFeature payload,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateFeatureCommand(User,
                payload.Name, payload.Description, payload.IsEnabled);
            var result = await _mediator.Send(command, cancellationToken);
            return Created($"v1/features/{result.Id}", result.MapFromDomain<Domain.Entities.Feature, Feature>());
        }

        [HttpDelete("{featureId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Remove([FromRoute] Guid featureId,
            CancellationToken cancellationToken = default)
        {
            await _mediator.Send(new DeleteFeatureCommand(User, featureId), cancellationToken);
            return NoContent();
        }

        [HttpPost("{featureId}/features/{dependId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AssignFeature(
            [FromRoute] Guid featureId,
            [FromRoute] Guid dependId,
            CancellationToken cancellationToken)
        {
            var command =
                new AssignFeatureCoDependencyCommand(User, featureId, dependId, LinkOperation.Assign);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{featureId}/features/{dependId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UnassignFeature(
            [FromRoute] Guid featureId,
            [FromRoute] Guid dependId,
            CancellationToken cancellationToken)
        {
            var command =
                new AssignFeatureCoDependencyCommand(User, featureId, dependId, LinkOperation.Unassign);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("{featureId}/permissions/{permissionId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AssignPermission(
            [FromRoute] Guid featureId,
            [FromRoute] Guid permissionId,
            CancellationToken cancellationToken)
        {
            var command =
                new AssignPermissionToFeatureCommand(User, featureId, permissionId, LinkOperation.Assign);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{featureId}/permissions/{permissionId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UnassignPermission(
            [FromRoute] Guid featureId,
            [FromRoute] Guid permissionId,
            CancellationToken cancellationToken)
        {
            var command =
                new AssignPermissionToFeatureCommand(User, featureId, permissionId, LinkOperation.Unassign);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        #endregion
    }
}