using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Infrastructure;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Consumers.Users;

public class UserUpdatedConsumer(
    RabbitMqConnectionFactory connectionFactory,
    IServiceScopeFactory scopeFactory,
    ILogger<UserUpdatedConsumer> logger)
    : RabbitMqConsumerBase<UserUpdatedEvent>(connectionFactory, scopeFactory, logger,
        MessagingConstants.Queues.ProjectUserUpdated)
{
    protected override async Task HandleAsync(UserUpdatedEvent message, Guid messageId, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        var inboxRepository = serviceProvider.GetRequiredService<IInboxRepository>();
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        if (await inboxRepository.IsProcessedAsync(
                messageId,
                nameof(UserUpdatedConsumer),
                cancellationToken))
        {
            return;
        }

        var user = await userRepository.FindAsync(u => u.Id == message.Id, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException(
                $"User {message.Id} must be created before applying an update.");
        }

        mapper.Map(message, user);

        await inboxRepository.AddAsync(
            messageId,
            nameof(UserUpdatedConsumer),
            cancellationToken);
        await userRepository.SaveAsync(cancellationToken);
    }
}
