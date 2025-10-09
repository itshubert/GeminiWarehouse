using ErrorOr;
using GeminiWarehouse.Application.Common.Interfaces;
using GeminiWarehouse.Application.Common.Models.Jobs;
using GeminiWarehouse.Domain.Common.Errors;
using GeminiWarehouse.Domain.JobAggregate;
using GeminiWarehouse.Domain.JobAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace GeminiWarehouse.Application.Jobs.Commands;

public sealed record CreateJobCommand(
    Guid FulfillmentId,
    Guid OrderId,
    string Status,
    string? TrackingNumber,
    ShippingAddressModel ShippingAddress,
    List<CreateJobItemRequest> JobItems) : IRequest<ErrorOr<JobModel>>;

public sealed record CreateJobItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity);

public sealed class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, ErrorOr<JobModel>>
{
    private readonly IJobRepository _jobRepository;
    private readonly IMapper _mapper;

    public CreateJobCommandHandler(IJobRepository jobRepository, IMapper mapper)
    {
        _jobRepository = jobRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<JobModel>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var jobFulfillment = await _jobRepository.GetJobByFulfillmentIdAsync(request.FulfillmentId, cancellationToken);

        if (jobFulfillment is not null)
        {
            return Errors.Job.JobFulfillmentAlreadyExists(request.FulfillmentId);
        }

        var jobOrder = await _jobRepository.GetJobByOrderIdAsync(request.OrderId, cancellationToken);

        if (jobOrder is not null)
        {
            return Errors.Job.JobOrderAlreadyExists(request.OrderId);
        }

        var job = Job.Create(
            null,
            request.FulfillmentId,
            request.OrderId,
            Domain.JobAggregate.JobStatus.Pending,
            ShippingAddress.Create(
                request.ShippingAddress.FirstName,
                request.ShippingAddress.LastName,
                request.ShippingAddress.AddressLine1,
                request.ShippingAddress.AddressLine2,
                request.ShippingAddress.City,
                request.ShippingAddress.State,
                request.ShippingAddress.PostCode,
                request.ShippingAddress.Country));

        await _jobRepository.AddAsync(job, cancellationToken);

        var jobModel = _mapper.Map<JobModel>(job);

        return jobModel;
    }
}