using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Infrastructure;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
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
    protected override async Task HandleAsync(UserCreatedEvent message, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();

        var exist = await userRepository.IsExistAsync(u => u.Id == message.Id, cancellationToken);
        if (exist)
        {
            return;
        }

        var user = new User
        {
            Id = message.Id,
            Email = message.Email,
            Name = message.Name,
            Surname = message.Surname,
            UserType = message.UserType,
            AvatarPath = message.AvatarPath,
            OrganizationId = message.OrganizationId
        };

        await userRepository.CreateAsync(user, cancellationToken);
    }
}