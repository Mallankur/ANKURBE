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

namespace Adform.Bloom.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class PoliciesController : DocumentedControllerBase
    {
        private readonly IMediator _mediator;

        public PoliciesController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Query

        [HttpGet("{policyId}")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(Policy), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromRoute] Guid policyId, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new PolicyQuery(User, policyId), cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(IEnumerable<Policy>), StatusCodes.Status200OK)]
        [ProducesResponseHeaders(Headers.Pagination, StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] Page page, CancellationToken cancellationToken = default)
        {
            var result =
                await _mediator.Send(new PoliciesQuery(User, new QueryParamsTenantIdsInput(), page.Offset, page.Limit),
                    cancellationToken);
            return new OkWithPaginationResult<IEnumerable<Policy>>(result.Data, page,
                new Order {OrderBy = Constants.Parameters.Id, OrderDirection = OrderDirection.Descending},
                page.ReturnTotalCount ? result.TotalItems : (long?) null);
        }

        #endregion

        #region Mutation

        [HttpPost]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(Policy), StatusCodes.Status201Created)]
        [SwaggerResponseHeader(StatusCodes.Status201Created, nameof(HeaderNames.Location), "string",
            "Location of resource")]
        public async Task<IActionResult> Create([FromBody] CreatePolicy payload,
            CancellationToken cancellationToken = default)
        {
            var command = new CreatePolicyCommand(User,
                payload.ParentId, payload.Name, payload.Description, payload.IsEnabled);
            var result = await _mediator.Send(command, cancellationToken);
            return Created($"v1/policies/{result.Id}", result.MapFromDomain<Domain.Entities.Policy, Policy>());
        }

        [HttpDelete("{policyId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Remove([FromRoute] Guid policyId,
            CancellationToken cancellationToken = default)
        {
            await _mediator.Send(new DeletePolicyCommand(User, policyId), cancellationToken);
            return NoContent();
        }

        #endregion
    }
}