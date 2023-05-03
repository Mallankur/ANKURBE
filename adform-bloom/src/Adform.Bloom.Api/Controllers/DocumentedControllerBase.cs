using Adform.Bloom.Api.Services;
using Adform.Bloom.Api.Swagger.Examples;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Adform.Bloom.Api.Controllers
{
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found", typeof(NotFoundErrorResponse))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorResponseExample))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(BadRequestErrorResponse))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestErrorResponseExample))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(UnauthorizedErrorResponse))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorResponseExample))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden", typeof(ForbiddenErrorResponse))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorResponseExample))]
    [SwaggerResponse(StatusCodes.Status406NotAcceptable, "Not Acceptable", typeof(ContentNegotiationFailureResponse))]
    [SwaggerResponseExample(StatusCodes.Status406NotAcceptable, typeof(ContentNegotiationFailureResponseExample))] 
    public abstract class DocumentedControllerBase : ControllerBase
    {
    }
}