using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeminiWarehouse.Api.Controllers;

[ApiController]
public abstract class BaseController : ApiController
{
    protected readonly ISender Mediator;
    protected readonly IMapper Mapper;

    protected BaseController(ISender mediator, IMapper mapper) : base(mediator, mapper)
    {
        Mediator = mediator;
        Mapper = mapper;
    }

    protected bool IsUserAuthenticated()
    {
        return HttpContext.User?.Identity?.IsAuthenticated ?? false;
    }

    protected Guid? GetUserId()
    {
        var userIdString = HttpContext.User?.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdString, out var userId) ? userId : null;
    }

    protected Guid? CustomerId()
    {
        var customerIdString = HttpContext.User?.FindFirst("customer_id")?.Value;
        return Guid.TryParse(customerIdString, out var customerId) ? customerId : null;
    }
}