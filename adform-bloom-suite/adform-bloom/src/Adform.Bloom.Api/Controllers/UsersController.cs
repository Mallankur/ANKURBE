using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Adform.AspNetCore.Paging;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.Swagger.Attributes;
using Adform.Ciam.Swagger.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Role = Adform.Bloom.Contracts.Output.Role;
using Subject = Adform.Bloom.Contracts.Output.Subject;

namespace Adform.Bloom.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class UsersController : DocumentedControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Query       
        [HttpGet("{subjectId}")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromRoute] Guid subjectId, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new UserQuery(User, subjectId), cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
        [ProducesResponseHeaders(Headers.Pagination, StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] Guid? tenantId, [FromQuery] Page page, [FromQuery]
            QueryParamsTenantIdsInput filter,
        CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new UsersQuery(User, filter, page.Offset, page.Limit), cancellationToken);
            return new OkWithPaginationResult<IEnumerable<User>>(result.Data, page,
                new Order { OrderBy = Constants.Parameters.Id, OrderDirection = OrderDirection.Descending },
                page.ReturnTotalCount ? result.TotalItems : (long?)null);
        }

        [HttpGet("{subjectId}/roles")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(IEnumerable<Role>), StatusCodes.Status200OK)]
        [ProducesResponseHeaders(Headers.Pagination, StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserRoles([FromRoute] Guid subjectId, [FromQuery] Page page, [FromQuery] QueryParamsTenantIdsInput? paramsInput, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new BaseAccessRangeQuery<Subject, QueryParamsTenantIdsInput,Role>(User,new Subject(){Id = subjectId}, page.Offset, page.Limit, paramsInput), cancellationToken);
            return new OkWithPaginationResult<IEnumerable<Role>>(result.Data, page,
                new Order { OrderBy = Constants.Parameters.Id, OrderDirection = OrderDirection.Descending },
                page.ReturnTotalCount ? result.TotalItems : (long?)null);
        }
        #endregion

        #region Mutation
        [HttpPost("{id}/business-accounts/{tenantId}/roles/{roleId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Assign(
            [FromRoute(Name = "id")] Guid subjectId,
            [FromRoute] Guid tenantId,
            [FromRoute] Guid roleId,
            CancellationToken cancellationToken)
        {
            var assignment = new List<RoleTenant>(){new RoleTenant
            {
                RoleId = roleId,
                TenantId = tenantId
            }};
            var command = new UpdateSubjectAssignmentsCommand(User, subjectId, assignment, null);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}/business-accounts/{tenantId}/roles/{roleId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Unassign(
            [FromRoute(Name = "id")] Guid subjectId,
            [FromRoute] Guid tenantId,
            [FromRoute] Guid roleId,
            CancellationToken cancellationToken)
        {
            var unassignment = new List<RoleTenant>(){new RoleTenant
            {
                RoleId = roleId,
                TenantId = tenantId
            }};
            var command = new UpdateSubjectAssignmentsCommand(User, subjectId, null, unassignment);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        #endregion
    }
}