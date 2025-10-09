using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Amazon.EventBridge;
using Amazon.SQS;
using Amazon.Runtime.CredentialManagement;
using Amazon;
using Microsoft.Extensions.Options;
using Amazon.Runtime;
using GeminiWarehouse.Infrastructure.Persistence;
using GeminiWarehouse.Application.Common.Messaging;
using GeminiWarehouse.Infrastructure.Messaging;
using GeminiWarehouse.Infrastructure.Interceptors;
using GeminiOrderFulfillment.Infrastructure.Messaging.Models;
using GeminiWarehouse.Application.Common.Interfaces;
using GeminiWarehouse.Infrastructure.Persistence.Repositories;

namespace GeminiOrderFulfillment.Infrastructure;

public static class DependencyInjectionRegister
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GeminiWarehouseDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("GeminiWarehouseDbContext");
            options.UseNpgsql(connectionString);
        });

        services.AddSingleton<IAmazonSQS>(sp =>
        {
            var useLocalStack = configuration.GetValue<bool>("AWS:UseLocalStack");

            if (useLocalStack)
            {
                var serviceUrl = configuration["AWS:LocalStack:ServiceUrl"] ?? "http://localhost:4566";
                var config = new AmazonSQSConfig { ServiceURL = serviceUrl };
                return new AmazonSQSClient(new AnonymousAWSCredentials(), config);
            }

            var profileName = Environment.GetEnvironmentVariable("AWS_PROFILE");
            if (!string.IsNullOrEmpty(profileName))
            {
                var credentialProfileStoreChain = new CredentialProfileStoreChain();
                if (credentialProfileStoreChain.TryGetProfile(profileName, out var profile))
                {
                    var credentials = profile.GetAWSCredentials(credentialProfileStoreChain);
                    return new AmazonSQSClient(credentials, RegionEndpoint.GetBySystemName("ap-southeast-2"));
                }
            }

            return new AmazonSQSClient(RegionEndpoint.GetBySystemName("ap-southeast-2"));
        });

        services.AddGeminiServices(configuration);
        ConfigureEventBridge(services, configuration);
        services.AddScoped<IEventBridgePublisher, EventBridgePublisher>();

        services.Configure<QueueSettings>(configuration.GetSection("QueueSettings"));

        // services.AddMessaging<InventoryReservedEvent, InventoryReservedEventProcessor>(sp =>
        // {
        //     return sp.GetRequiredService<IOptions<QueueSettings>>().Value.InventoryReserved ?? string.Empty;
        // });

        services.AddScoped<PublishDomainEventsInterceptor>();
        services.AddScoped<IJobRepository, JobRepository>();

        return services;
    }

    private static IServiceCollection AddGeminiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {


        services.Configure<ServicesSettings>(configuration.GetSection("Services"));

        Random jitterer = new();

        var servicesSettings = new ServicesSettings();
        configuration.GetSection("Services").Bind(servicesSettings);

        // SetupHttpClient<CatalogServiceClient>(
        //     services,
        //     servicesSettings.CatalogServiceBaseUrl,
        //     jitterer);

        // SetupHttpClient<CustomerServiceClient>(
        //     services,
        //     servicesSettings.CustomerServiceBaseUrl,
        //     jitterer);

        return services;
    }

    private static void SetupHttpClient<T>(
        IServiceCollection services,
        string baseUrl,
        Random jitterer) where T : class
    {
        services.AddHttpClient<T>((serviceProvider, client) =>
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException($"Base URL cannot be null or empty for type {typeof(T).Name}");
            }

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler(options =>
        {
            // Configure retry with exponential backoff and jitter
            options.Retry.MaxRetryAttempts = 4;
            options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;
            options.Retry.Delay = TimeSpan.FromSeconds(2);

            // Configure circuit breaker
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 5;

            // Configure timeout
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });
    }

    private static void ConfigureEventBridge(IServiceCollection services, IConfiguration configuration)
    {
        var useLocalStack = configuration.GetValue<bool>("AWS:UseLocalStack");

        if (useLocalStack)
        {
            // Configure for LocalStack
            var serviceUrl = configuration["AWS:LocalStack:ServiceUrl"] ?? "http://localhost:4566";

            services.AddAWSService<IAmazonEventBridge>(new Amazon.Extensions.NETCore.Setup.AWSOptions
            {
                Credentials = new AnonymousAWSCredentials(),
                DefaultClientConfig =
                {
                    ServiceURL = serviceUrl
                }
            });

            // Log LocalStack configuration
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<IAmazonEventBridge>>();
                logger.LogInformation("EventBridge configured for LocalStack at {ServiceUrl}", serviceUrl);
                return sp;
            });
        }
        else
        {
            // Configure for AWS - use environment variables or IAM roles
            var region = configuration["AWS:Region"] ?? "ap-southeast-2";
            services.AddAWSService<IAmazonEventBridge>(new Amazon.Extensions.NETCore.Setup.AWSOptions
            {
                Region = RegionEndpoint.GetBySystemName(region)
            });

            // Log AWS configuration
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<IAmazonEventBridge>>();
                logger.LogInformation("EventBridge configured for AWS in region {Region}", region);
                return sp;
            });
        }
    }

    public static IServiceCollection AddMessaging<TEvent, TProcessor>(
        this IServiceCollection services,
        Func<IServiceProvider, string> queueUrlFactory)
            where TProcessor : class, IEventProcessor<TEvent>
    {


        services.AddScoped<IEventProcessor<TEvent>, TProcessor>();

        services.AddHostedService(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SqsConsumerBackgroundService<TEvent, TProcessor>>>();
            var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var sqs = sp.GetRequiredService<IAmazonSQS>();
            var queueUrl = queueUrlFactory(sp);

            logger.LogInformation("Starting SQS consumer for queue: {QueueUrl}", queueUrl);

            return new SqsConsumerBackgroundService<TEvent, TProcessor>(
                logger,
                serviceScopeFactory,
                sqs,
                queueUrl);
        });

        return services;
    }
}