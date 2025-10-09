using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Amazon.Runtime;
using GeminiWarehouse.Application.Common.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace GeminiWarehouse.Infrastructure.Messaging;

public sealed class EventBridgePublisher : IEventBridgePublisher
{
    private readonly IAmazonEventBridge _eventBridgeClient;
    private readonly ILogger<EventBridgePublisher> _logger;
    private readonly string _eventBusName;
    private readonly string _source;
    private readonly ResiliencePipeline _resiliencePipeline;

    public EventBridgePublisher(
        IAmazonEventBridge eventBridgeClient,
        IConfiguration configuration,
        ILogger<EventBridgePublisher> logger)
    {
        _eventBridgeClient = eventBridgeClient;
        _logger = logger;
        _eventBusName = configuration["AWS:EventBridge:EventBusName"] ?? "gemini";
        _source = configuration["AWS:EventBridge:Source"] ?? "gemini";

        // Build resilience pipeline with retry, circuit breaker, and timeout
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<AmazonEventBridgeException>()
                    .Handle<AmazonServiceException>()
                    .Handle<InvalidOperationException>(),
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Retry attempt {AttemptNumber} for EventBridge publish. Delay: {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    _logger.LogError(
                        "Circuit breaker opened for EventBridge publisher. Will retry after {BreakDuration}s",
                        args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("Circuit breaker closed for EventBridge publisher");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    _logger.LogInformation("Circuit breaker half-opened for EventBridge publisher");
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(30))
            .Build();
    }

    public async Task PublishAsync<T>(
        DetailTypes detailType,
        T eventDetail,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                var eventDetailJson = JsonSerializer.Serialize(eventDetail, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var putEventsRequest = new PutEventsRequest
                {
                    Entries = new List<PutEventsRequestEntry>
                    {
                        new PutEventsRequestEntry
                        {
                            Source = _source,
                            DetailType = detailType.ToString(),
                            Detail = eventDetailJson,
                            EventBusName = _eventBusName,
                            Time = DateTime.UtcNow
                        }
                    }
                };

                var response = await _eventBridgeClient.PutEventsAsync(putEventsRequest, ct);

                if (response.FailedEntryCount > 0)
                {
                    var failedEntry = response.Entries.FirstOrDefault(e => e.ErrorCode != null);
                    _logger.LogError(
                        "Failed to publish event to EventBridge. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                        failedEntry?.ErrorCode,
                        failedEntry?.ErrorMessage);

                    throw new InvalidOperationException(
                        $"Failed to publish event to EventBridge: {failedEntry?.ErrorMessage}");
                }

                _logger.LogInformation(
                    "Successfully published event to EventBridge. Source: {Source}, DetailType: {DetailType}, EventBus: {EventBus}",
                    _source,
                    detailType,
                    _eventBusName);
            }, cancellationToken);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex,
                "Circuit breaker is open. Cannot publish event to EventBridge. DetailType: {DetailType}",
                detailType);
            throw;
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex,
                "Timeout publishing event to EventBridge. DetailType: {DetailType}",
                detailType);
            throw;
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex,
                "EventBridge publish operation was canceled. DetailType: {DetailType}",
                detailType);
            throw;
        }
        catch (AmazonEventBridgeException ex)
        {
            _logger.LogError(ex,
                "AWS EventBridge error publishing event. DetailType: {DetailType}, ErrorCode: {ErrorCode}, StatusCode: {StatusCode}",
                detailType,
                ex.ErrorCode,
                ex.StatusCode);
            throw;
        }
        catch (AmazonServiceException ex)
        {
            _logger.LogError(ex,
                "AWS Service error publishing event. DetailType: {DetailType}, ErrorCode: {ErrorCode}, StatusCode: {StatusCode}",
                detailType,
                ex.ErrorCode,
                ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing event to EventBridge. DetailType: {DetailType}",
                detailType);
            throw;
        }
    }
}