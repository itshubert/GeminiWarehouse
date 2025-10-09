using GeminiWarehouse.Application.Common.Interfaces;
using GeminiWarehouse.Application.Common.Models.Jobs;
using MapsterMapper;
using MediatR;

namespace GeminiWarehouse.Application.Jobs.Commands;

public sealed record CreateJobCommand(
    Guid FulfillmentId,
    Guid OrderId,
    string Status,
    string? TrackingNumber,
    ShippingAddressModel ShippingAddress,
    List<CreateJobItemRequest> JobItems) : IRequest<JobModel>;

public sealed record CreateJobItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity);

public sealed class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, JobModel>
{
    private readonly IJobRepository _jobRepository;
    private readonly IMapper _mapper;

    public CreateJobCommandHandler(IJobRepository jobRepository, IMapper mapper)
    {
        _jobRepository = jobRepository;
        _mapper = mapper;
    }

    public async Task<JobModel> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}