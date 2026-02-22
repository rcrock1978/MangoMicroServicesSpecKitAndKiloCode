using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Mango.MessageBus;

/// <summary>
/// Message bus implementation using MassTransit with RabbitMQ
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publish an integration event to the message bus
    /// </summary>
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publish an integration event with a specific correlation ID
    /// </summary>
    Task PublishAsync<T>(T message, Guid correlationId, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Request a response from a service
    /// </summary>
    Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;
}

/// <summary>
/// Base class for all integration events
/// </summary>
public abstract class IntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
}

/// <summary>
/// MassTransit implementation of the message bus
/// </summary>
public class MessageBus : IMessageBus, IDisposable
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<IRequest> _requestClient;
    private readonly ILogger<MessageBus> _logger;
    private bool _disposed;

    public MessageBus(IPublishEndpoint publishEndpoint, ILogger<MessageBus> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public MessageBus(IPublishEndpoint publishEndpoint, IRequestClient<IRequest> requestClient, ILogger<MessageBus> logger)
    {
        _publishEndpoint = publishEndpoint;
        _requestClient = requestClient;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            if (message is IntegrationEvent integrationEvent)
            {
                _logger.LogInformation(
                    "Publishing event {EventType} with CorrelationId {CorrelationId}",
                    typeof(T).Name,
                    integrationEvent.CorrelationId);
            }

            await _publishEndpoint.Publish(message, cancellationToken);

            _logger.LogDebug(
                "Published event {EventType} successfully",
                typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType}", typeof(T).Name);
            throw;
        }
    }

    public async Task PublishAsync<T>(T message, Guid correlationId, CancellationToken cancellationToken = default)
        where T : class
    {
        if (message is IntegrationEvent integrationEvent)
        {
            integrationEvent.CorrelationId = correlationId;
        }

        await PublishAsync(message, cancellationToken);
    }

    public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        throw new NotImplementedException("Request/Response pattern requires IRequestClient to be configured");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}

/// <summary>
/// Request marker interface
/// </summary>
public interface IRequest
{
    Guid CorrelationId { get; }
}

/// <summary>
/// Base class for all integration events with common properties
/// </summary>
public abstract class IntegrationEventBase : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Interface for integration events
/// </summary>
public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime CreatedAt { get; }
    Guid CorrelationId { get; set; }
}
