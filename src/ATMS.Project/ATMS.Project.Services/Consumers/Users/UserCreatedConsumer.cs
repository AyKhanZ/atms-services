using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Infrastructure;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Consumers.Users;

public sealed class UserCreatedConsumer(
    RabbitMqConnectionFactory connectionFactory,
    IServiceScopeFactory scopeFactory,
    ILogger<UserCreatedConsumer> logger)
    : RabbitMqConsumerBase<UserCreatedEvent>(connectionFactory, scopeFactory, logger,
        MessagingConstants.Queues.ProjectUserCreated)
{
    protected override async Task HandleAsync(UserCreatedEvent message, Guid messageId, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        var inboxRepository = serviceProvider.GetRequiredService<IInboxRepository>();
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        if (await inboxRepository.IsProcessedAsync(
                messageId,
                nameof(UserCreatedConsumer),
                cancellationToken))
        {
            return;
        }

        var exist = await userRepository.IsExistAsync(u => u.Id == message.Id, cancellationToken);
        if (exist)
        {
            await inboxRepository.AddAsync(
                messageId,
                nameof(UserCreatedConsumer),
                cancellationToken);
            await userRepository.SaveAsync(cancellationToken);
            return;
        }

        var user = mapper.Map<User>(message);

        await userRepository.AddAsync(user, cancellationToken);
        await inboxRepository.AddAsync(
            messageId,
            nameof(UserCreatedConsumer),
            cancellationToken);
        await userRepository.SaveAsync(cancellationToken);
    }
}
