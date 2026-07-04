using FCG.Users.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Users.Infrastructure.Messaging;

public class EventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(IPublishEndpoint publishEndpoint, ILogger<EventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T evento) where T : class
    {
        _logger.LogInformation("Publicando evento {TipoEvento}", typeof(T).Name);
        await _publishEndpoint.Publish(evento);
    }
}
