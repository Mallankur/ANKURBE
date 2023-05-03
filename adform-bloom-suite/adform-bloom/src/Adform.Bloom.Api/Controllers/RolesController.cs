using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Adform.AspNetCore.Paging;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Domain.Extensions;
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
    public class RolesController : DocumentedControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Query

        [HttpGet("{roleId}")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(Role), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromRoute] Guid roleId, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new RoleQuery(User, roleId), cancellationToken);
            return Ok(result);
        }

#warning 2021-11-24 Sorting fix APF-12710 not applied as there's no sorting on REST endpoint at all
        [HttpGet]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(IEnumerable<Role>), StatusCodes.Status200OK)]
        [ProducesResponseHeaders(Headers.Pagination, StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] Page page, CancellationToken cancellationToken = default)
        {
            var result =
                await _mediator.Send(new RolesQuery(User, new QueryParamsRolesInput(), page.Offset, page.Limit),
                    cancellationToken);
            return new OkWithPaginationResult<IEnumerable<Role>>(result.Data, page,
                new Order {OrderBy = Constants.Parameters.Id, OrderDirection = OrderDirection.Descending},
                page.ReturnTotalCount ? result.TotalItems : (long?) null);
        }

        [HttpGet("template")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(IEnumerable<Role>), StatusCodes.Status200OK)]
        [ProducesResponseHeaders(Headers.Pagination, StatusCodes.Status200OK)]
        public async Task<IActionResult> ListTemplateRoles([FromQuery] Page page,
            CancellationToken cancellationToken = default)
        {
            var result =
                await _mediator.Send(new RolesQuery(User, new QueryParamsRolesInput(), page.Offset, page.Limit),
                    cancellationToken);
            return new OkWithPaginationResult<IEnumerable<Role>>(result.Data, page,
                new Order {OrderBy = Constants.Parameters.Id, OrderDirection = OrderDirection.Descending},
                page.ReturnTotalCount ? result.TotalItems : (long?) null);
        }

        #endregion

        #region Mutation

        [HttpPost]
        [Authorize(StartupOAuth.Scopes.Full)]
        [RoleAuthorize(ClaimPrincipalExtensions.AdformAdmin, ClaimPrincipalExtensions.LocalAdmin)]
        [ProducesResponseType(typeof(Role), StatusCodes.Status201Created)]
        [SwaggerResponseHeader(StatusCodes.Status201Created, nameof(HeaderNames.Location), "string",
            "Location of resource")]
        public async Task<IActionResult> Create([FromBody] CreateRole payload,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateRoleCommand(User,
                payload.PolicyId,
                payload.TenantId,
                payload.Name,
                payload.Description,
                payload.IsEnabled);
            var result = await _mediator.Send(command, cancellationToken);
            return Created($"v1/roles/{result.Id}", result.MapFromDomain<Domain.Entities.Role, Role>());
        }

        [HttpDelete("{roleId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [RoleAuthorize(ClaimPrincipalExtensions.AdformAdmin, ClaimPrincipalExtensions.LocalAdmin)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Remove([FromRoute] Guid roleId, CancellationToken cancellationToken = default)
        {
            await _mediator.Send(new DeleteRoleCommand(User, roleId), cancellationToken);
            return NoContent();
        }

        [HttpPost("{id}/permissions/{permissionId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [RoleAuthorize(ClaimPrincipalExtensions.AdformAdmin)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Assign([FromRoute] Guid permissionId,
            [FromRoute(Name = "id")] Guid roleId,
            CancellationToken cancellationToken)
        {
            var command =
                new AssignPermissionToRoleCommand(User, permissionId, roleId, LinkOperation.Assign);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }


        [HttpDelete("{id}/permissions/{permissionId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [RoleAuthorize(ClaimPrincipalExtensions.AdformAdmin)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Unassign([FromRoute] Guid permissionId,
            [FromRoute(Name = "id")] Guid roleId,
            CancellationToken cancellationToken)
        {
            var command =
                new AssignPermissionToRoleCommand(User, permissionId, roleId, LinkOperation.Unassign);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        #endregion
    }

    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        public RoleAuthorizeAttribute(params string[] roles) => this.Roles = string.Join(",", roles);
    }
}