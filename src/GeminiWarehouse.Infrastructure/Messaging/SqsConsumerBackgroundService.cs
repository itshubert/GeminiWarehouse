using System.Text.Json;
using System.Threading.Channels;
using Amazon.SQS;
using Amazon.SQS.Model;
using GeminiWarehouse.Application.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GeminiWarehouse.Infrastructure.Messaging;

public sealed class SqsConsumerBackgroundService<TEvent, TProcessor> : BackgroundService
    where TProcessor : class,
    IEventProcessor<TEvent>
{
    private readonly ILogger<SqsConsumerBackgroundService<TEvent, TProcessor>> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IAmazonSQS _sqs;
    private readonly string _queueUrl;

    public SqsConsumerBackgroundService(
        ILogger<SqsConsumerBackgroundService<TEvent, TProcessor>> logger,
        IServiceScopeFactory serviceScopeFactory,
        IAmazonSQS sqs,
        string queueUrl)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _sqs = sqs;
        _queueUrl = queueUrl;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = Channel.CreateUnbounded<Message>();
        var writer = channel.Writer;
        var reader = channel.Reader;

        await Task.WhenAll(
            StartMessagePollingAsync(writer, stoppingToken),
            ProcessMessageAsync(reader, stoppingToken)
        );
    }

    private async Task StartMessagePollingAsync(ChannelWriter<Message> writer, CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Poll for messages from the queue
                var messages = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20
                }, stoppingToken);

                if (messages.Messages is null)
                {
                    continue;
                }

                foreach (var message in messages.Messages)
                {
                    // Push message to the channel
                    await writer.WriteAsync(message, stoppingToken);
                }
            }
        }
        finally
        {
            writer.Complete();
        }
    }

    private async Task ProcessMessageAsync(ChannelReader<Message> reader, CancellationToken stoppingToken)
    {
        // Configure JSON options for case-insensitive deserialization
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var readerTasks = Enumerable.Range(0, 5).Select(async (index) =>
        {
            _logger.LogInformation("Starting message processing for reader {Index}", index);

            while (await reader.WaitToReadAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IEventProcessor<TEvent>>();

                    var message = await reader.ReadAsync(stoppingToken);

                    // Deserialize the EventBridge envelope first, then extract the detail
                    // var envelope = JsonSerializer.Deserialize<EventBridgeEnvelope<TEvent>>(message.Body, jsonOptions);
                    var obj = JsonSerializer.Deserialize<TEvent>(message.Body, jsonOptions);

                    // if (envelope is not null && envelope.Detail is not null)
                    if (obj is not null)
                    {
                        var success = await processor.ProcessEventAsync(obj, stoppingToken);

                        // Delete the message from the queue after successful processing
                        if (success)
                        {
                            await _sqs.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from Index {Index}", index);
                }
            }

        });

        await Task.WhenAll(readerTasks);
    }
}