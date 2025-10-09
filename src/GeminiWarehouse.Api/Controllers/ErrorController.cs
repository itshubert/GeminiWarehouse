using ErrorOr;
using GeminiWarehouse.Api.Common.Http;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GeminiWarehouse.Api.Controllers;

public class ErrorController : ApiController
{
    private readonly IHostEnvironment _env;

    public ErrorController(ISender mediator, IMapper mapper, IHostEnvironment env) : base(mediator, mapper)
    {
        _env = env;
    }

    [Route("/error")]
    [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch] // Handle all HTTP methods
    public IActionResult Error()
    {
        var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

        // Check if we have structured errors in context
        if (HttpContext.Items.TryGetValue(HttpContextItemKeys.Errors, out var errorsObj)
            && errorsObj is List<Error> errors)
        {
            return Problem(errors);
        }

        // Handle different exception types
        return exception switch
        {
            ArgumentException => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Invalid request parameters"),
            UnauthorizedAccessException => Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: _env.IsDevelopment() ? exception?.Message : "An error occurred"),
            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: _env.IsDevelopment() ? exception?.Message : "An error occurred")
        };
    }
}