using GeminiWarehouse.Application.Jobs.Commands;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeminiWarehouse.Api.Controllers;

[Microsoft.AspNetCore.Mvc.Route("[controller]")]
public class JobsController : BaseController
{
    public JobsController(
        ISender mediator,
        IMapper mapper) : base(mediator, mapper)
    { }

    [HttpPost("{jobId}/status-update/{newStatus}")]
    public async Task<IActionResult> UpdateJobStatus(Guid jobId, string newStatus)
    {
        var command = new UpdateJobStatusCommand(jobId, newStatus.ToLower() switch
        {
            "pending" => Application.Common.Models.Jobs.JobStatus.Pending,
            "inprogress" => Application.Common.Models.Jobs.JobStatus.InProgress,
            "completed" => Application.Common.Models.Jobs.JobStatus.Completed,
            "cancelled" => Application.Common.Models.Jobs.JobStatus.Cancelled,
            _ => throw new ArgumentException("Invalid status", nameof(newStatus))
        });

        await Mediator.Send(command);

        return NoContent();
    }

}