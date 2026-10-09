using ATMS.Caching.Services.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Infrastructure;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Consumers.Users;

public class UserStatusChangedConsumer(
    RabbitMqConnectionFactory connectionFactory,
    IServiceScopeFactory scopeFactory,
    ILogger<UserStatusChangedConsumer> logger)
    : RabbitMqConsumerBase<UserStatusChangedEvent>(connectionFactory, scopeFactory, logger,
        MessagingConstants.Queues.ProjectUserStatusChanged)
{
    protected override async Task HandleAsync(UserStatusChangedEvent message, Guid messageId, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        var inboxRepository = serviceProvider.GetRequiredService<IInboxRepository>();
        var workProjectRepository = serviceProvider.GetRequiredService<IWorkProjectRepository>();
        var cache = serviceProvider.GetRequiredService<ICacheService>();

        if (await inboxRepository.IsProcessedAsync(
                messageId,
                nameof(UserStatusChangedConsumer),
                cancellationToken))
        {
            return;
        }

        var user = await userRepository.FindAsync(u => u.Id == message.Id, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException(
                $"User {message.Id} must be created before applying a status change.");
        }

        user.IsActive = message.IsActive;

        await inboxRepository.AddAsync(
            messageId,
            nameof(UserStatusChangedConsumer),
            cancellationToken);
        await userRepository.SaveAsync(cancellationToken);

        // project details cache the participant for 5 min, so a status change shows up
        foreach (var projectId in await workProjectRepository.GetIdsByParticipantAsync(user.Id, cancellationToken))
        {
            await cache.RemoveWorkProjectAsync(projectId, cancellationToken);
        }
    }
}
