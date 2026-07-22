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
    ILogger<UserCreatedConsumer> logger)
    : RabbitMqConsumerBase<UserUpdatedEvent>(connectionFactory, scopeFactory, logger,
        MessagingConstants.Queues.ProjectUserUpdated)
{
    protected override async Task HandleAsync(UserUpdatedEvent message, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var user = await userRepository.FindAsync(u => u.Id == message.Id, cancellationToken);
        if (user is null)
        {
            return;
        }

        mapper.Map(message, user);

        await userRepository.SaveAsync(cancellationToken);
    }
}
