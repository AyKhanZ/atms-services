using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Infrastructure;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Consumers.Users;

public class UserUpdatedConsumer(
    RabbitMqConnectionFactory connectionFactory,
    IServiceScopeFactory scopeFactory,
    ILogger<UserCreatedConsumer> logger)
    : RabbitMqConsumerBase<UserUpdatedEvent>(connectionFactory, scopeFactory, logger,
        MessagingConstants.Queues.ProjectUserCreated)
{
    protected override async Task HandleAsync(UserUpdatedEvent message, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        
        var user = await userRepository.FindAsync(u => u.Id == message.Id, cancellationToken);
        if (user is null)
        {
            return;
        }

        user.Name = message.Name;
        user.Surname = message.Surname;
        user.AvatarPath = message.AvatarPath;

        await userRepository.SaveAsync(cancellationToken);
    }
}