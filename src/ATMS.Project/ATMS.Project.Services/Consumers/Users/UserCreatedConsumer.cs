using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Infrastructure;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Invitations.Interfaces;
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
        var invitationService = serviceProvider.GetRequiredService<IWorkProjectInvitationService>();
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        if (await inboxRepository.IsProcessedAsync(
                messageId,
                nameof(UserCreatedConsumer),
                cancellationToken))
        {
            return;
        }

        var user = await userRepository.FindAsync(u => u.Id == message.Id, cancellationToken);
        if (user is not null)
        {
            mapper.Map(message, user);
        }
        else
        {
            user = mapper.Map<User>(message);
            await userRepository.AddAsync(user, cancellationToken);
        }

        // Before the inbox record: if settling fails, the retry must find the message unprocessed
        // and settle what is still pending. An existing user counts too — Admin announces one again
        // when an invitation arrived for an email that was registered a moment before.
        await invitationService.SettlePendingAsync(user, cancellationToken);

        await inboxRepository.AddAsync(
            messageId,
            nameof(UserCreatedConsumer),
            cancellationToken);
        await userRepository.SaveAsync(cancellationToken);
    }
}
