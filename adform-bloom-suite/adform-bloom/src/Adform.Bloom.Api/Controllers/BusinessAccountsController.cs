using Adform.AspNetCore.Paging;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Read.Queries;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.Swagger.Attributes;
using Adform.Ciam.Swagger.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Microsoft.AspNetCore.Authorization;

namespace Adform.Bloom.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/business-accounts")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class BusinessAccountsController : DocumentedControllerBase
    {
        private readonly IMediator _mediator;

        public BusinessAccountsController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Query

        [HttpGet("{businessAccountId}")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(BusinessAccount), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromRoute] Guid businessAccountId,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new BusinessAccountQuery(User, businessAccountId), cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(IEnumerable<BusinessAccount>), StatusCodes.Status200OK)]
        [ProducesResponseHeaders(Headers.Pagination, StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] Page page, CancellationToken cancellationToken = default)
        {
            var result =
                await _mediator.Send(
                    new BusinessAccountsQuery(User, new QueryParamsBusinessAccountInput(), page.Offset, page.Limit),
                    cancellationToken);
            return new OkWithPaginationResult<IEnumerable<BusinessAccount>>(result.Data, page,
                new Order {OrderBy = Constants.Parameters.Id, OrderDirection = OrderDirection.Descending},
                page.ReturnTotalCount ? result.TotalItems : (long?) null);
        }

        #endregion

        #region Mutation

        [HttpPost("{id}/licensedFeatures/{licensedFeatureId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AssignToLicensedFeature(
            [FromRoute(Name = "id")] Guid businessAccountId,
            [FromRoute] Guid licensedFeatureId,
            CancellationToken cancellationToken)
        {
            var command =
                new UpdateLicensedFeatureToTenantAssignmentsCommand(User, businessAccountId, new []{ licensedFeatureId }, null);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}/licensedFeatures/{licensedFeatureId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UnassignFromLicensedFeature(
            [FromRoute(Name = "id")] Guid businessAccountId,
            [FromRoute] Guid licensedFeatureId,
            CancellationToken cancellationToken)
        {
            var command =
                new UpdateLicensedFeatureToTenantAssignmentsCommand(User, businessAccountId, null, new[] { licensedFeatureId });
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        #endregion
    }
}