using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Application.Extensions;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Application.Validators;
using Adform.Bloom.Runtime.Host.Capabilities;
using Adform.Bloom.Runtime.Host.Swagger;
using Adform.Bloom.Runtime.Read.Entities;
using Adform.Ciam.ExceptionHandling.Abstractions.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Adform.Bloom.Runtime.Host.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public class RuntimeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IExistenceQueryValidator _validator;

        public RuntimeController(
            IMediator mediator, IExistenceQueryValidator validator)
        {
            _mediator = mediator;
            _validator = validator;
        }

        [HttpPost("subject-evaluation")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(IEnumerable<RuntimeResult>), StatusCodes.Status200OK)]
        [SwaggerRequestExample(typeof(SubjectRuntimeQuery), typeof(SubjectRuntimeQueryExample))]
        public async Task<IActionResult> Evaluate([FromBody]SubjectRuntimeQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("nodes-exist")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(ExistenceResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> NodesExist([FromBody] NodeExistenceQuery query, CancellationToken cancellationToken = default)
        {
            var validationResult = _validator.Validate(query);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToString());
            }

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(new ExistenceResult { Exists = result.ThrowIfException()});
        }

        [HttpPost("legacy-tenants-exist")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(ExistenceResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> LegacyTenantsExist([FromBody] LegacyTenantExistenceQuery query, CancellationToken cancellationToken = default)
        {
            var validationResult = _validator.Validate(query);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToString());
            }

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(new ExistenceResult { Exists = result.ThrowIfException() });
        }

        [HttpPost("role-exists")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(ExistenceResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> RoleExists([FromBody] RoleExistenceQuery query, CancellationToken cancellationToken = default)
        {
            var validationResult = _validator.Validate(query);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToString());
            }

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(new ExistenceResult { Exists = result.ThrowIfException() });
        }

        [HttpPost("subject-intersection")]
        [Authorize(StartupOAuth.Scopes.Readonly)]
        [ProducesResponseType(typeof(IEnumerable<RuntimeResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Intersect([FromBody] SubjectIntersectionQuery query, CancellationToken cancellationToken = default)
        {
            var validationResult = _validator.Validate(query);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToString());
            }
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}