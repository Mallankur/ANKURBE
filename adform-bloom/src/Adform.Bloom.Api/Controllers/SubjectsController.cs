using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Services;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Write.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Filters;
using Subject = Adform.Bloom.Contracts.Output.Subject;

namespace Adform.Bloom.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class SubjectsController : DocumentedControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IClaimPrincipalGenerator _claimPrincipalGenerator;

        public SubjectsController(
            IMediator mediator,
            IClaimPrincipalGenerator claimPrincipalGenerator)
        {
            _mediator = mediator;
            _claimPrincipalGenerator = claimPrincipalGenerator;
        }

        #region Mutation
        [HttpPost]
        [Authorize(StartupOAuth.Scopes.FullSubject)]
        [ProducesResponseType(typeof(Subject), StatusCodes.Status201Created)]
        [SwaggerResponseHeader(StatusCodes.Status201Created, nameof(HeaderNames.Location), "string", "Location of resource")]
        public async Task<IActionResult> Create([FromBody] CreateSubject payload,
            CancellationToken cancellationToken = default)
        {
            var assignedRoles = payload.RoleBusinessAccounts?.Select(o => new RoleTenant
            {
                RoleId = o.RoleId,
                TenantId = o.BusinessAccountId
            }).ToList();

            var actor = await _claimPrincipalGenerator.GenerateAsync(payload.ActorId, null, cancellationToken);

            var command = new CreateSubjectCommand(actor,
                payload.Id,
                payload.Email,
                payload.IsEnabled,
                assignedRoles);
            var result = await _mediator.Send(command, cancellationToken);
            return Created($"v1/subjects/{result.Id}", result.MapFromDomain());
        }

        [HttpDelete("{subjectId}")]
        [Authorize(StartupOAuth.Scopes.Full)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Remove([FromRoute] Guid subjectId,
            CancellationToken cancellationToken = default)
        {
            await _mediator.Send(new DeleteSubjectCommand(User, subjectId), cancellationToken);
            return NoContent();
        }

        #endregion
    }
}