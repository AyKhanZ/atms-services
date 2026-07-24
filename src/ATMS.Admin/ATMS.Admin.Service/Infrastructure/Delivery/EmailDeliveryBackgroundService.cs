using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Enums;
using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Admin.Service.Infrastructure.Delivery;

public class EmailDeliveryBackgroundService(
    IServiceScopeFactory scopeFactory,
    DeliveryRetrySchedule retrySchedule,
    IConfiguration configuration,
    ILogger<EmailDeliveryBackgroundService> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan EmptyQueueDelay = TimeSpan.FromSeconds(5);

    private readonly RedirectUrlOptions _redirectUrlOptions =
        configuration.GetSection(nameof(RedirectUrlOptions)).Get<RedirectUrlOptions>()
        ?? throw new ConfigurationException(
            ConfigurationErrorType.RedirectUrlSectionNotFound,
            string.Format(LogMessages.ConfigSectionNotFound, nameof(RedirectUrlOptions)));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedCount = await ProcessBatchAsync(stoppingToken);
                if (processedCount == 0)
                {
                    await Task.Delay(EmptyQueueDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Email delivery worker failed while loading a delivery batch");
                await Task.Delay(EmptyQueueDelay, stoppingToken);
            }
        }
    }

    protected virtual async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        List<Admin.Data.Entities.Messaging.EmailDelivery> deliveries;
        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IEmailDeliveryRepository>();
            deliveries = await repository.ClaimPendingAsync(BatchSize, cancellationToken);
        }

        foreach (var delivery in deliveries)
        {
            await ProcessDeliveryAsync(delivery.Id, delivery.AttemptCount, cancellationToken);
        }

        return deliveries.Count;
    }

    private async Task ProcessDeliveryAsync(
        Guid deliveryId,
        int previousAttemptCount,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEmailDeliveryRepository>();
            var currentDelivery = await repository.GetAsync(deliveryId, cancellationToken);
            if (currentDelivery is null || currentDelivery.Status != DeliveryStatusEnum.Pending)
            {
                return;
            }

            if (currentDelivery.Type == EmailDeliveryTypeEnum.Confirmation)
            {
                await SendConfirmationAsync(currentDelivery, scope.ServiceProvider, cancellationToken);
            }
            else
            {
                await SendPasswordResetAsync(
                    currentDelivery,
                    repository,
                    scope.ServiceProvider,
                    cancellationToken);
            }

            await repository.MarkProcessedAsync(currentDelivery.Id, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await HandleFailureAsync(deliveryId, previousAttemptCount, exception, cancellationToken);
        }
    }

    private async Task HandleFailureAsync(
        Guid deliveryId,
        int previousAttemptCount,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmailDeliveryRepository>();
        var delivery = await repository.GetAsync(deliveryId, cancellationToken);
        if (delivery is null || delivery.Status != DeliveryStatusEnum.Pending)
        {
            return;
        }

        var attemptCount = previousAttemptCount + 1;
        logger.LogError(
            exception,
            "Email delivery {DeliveryId} failed on attempt {AttemptCount}",
            deliveryId,
            attemptCount);

        if (attemptCount >= retrySchedule.MaxAttemptCount)
        {
            await repository.MarkFailedAsync(deliveryId, attemptCount, exception.Message, cancellationToken);
            return;
        }

        await repository.MarkRetryAsync(
            deliveryId,
            attemptCount,
            retrySchedule.GetNextAttemptAt(attemptCount),
            exception.Message,
            cancellationToken);
    }

    private async Task SendConfirmationAsync(
        Admin.Data.Entities.Messaging.EmailDelivery delivery,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (delivery.TemporaryPassword is null)
        {
            throw new InvalidOperationException("The confirmation delivery has no temporary password.");
        }

        var tokenService = serviceProvider.GetRequiredService<IEmailConfirmationTokenService>();
        var emailSender = serviceProvider.GetRequiredService<IEmailSender>();
        var tokenResult = tokenService.GenerateToken(delivery.User);
        var link =
            $"{_redirectUrlOptions.BaseUrl}/account/confirm?token={Uri.EscapeDataString(tokenResult.Token)}";

        await emailSender.SendAsync(
            delivery.User.Email,
            new InviteModel
            {
                Email = delivery.User.Email,
                Name = delivery.User.Name,
                Surname = delivery.User.Surname,
                Password = delivery.TemporaryPassword,
                Link = link,
                DeadlineOfToken = tokenResult.ExpiresInHours
            },
            cancellationToken);
    }

    private async Task SendPasswordResetAsync(
        Admin.Data.Entities.Messaging.EmailDelivery delivery,
        IEmailDeliveryRepository repository,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var emailSender = serviceProvider.GetRequiredService<IEmailSender>();
        var tokenResult = await GetPasswordResetTokenAsync(
            delivery,
            repository,
            serviceProvider,
            cancellationToken);
        var link =
            $"{_redirectUrlOptions.ResetPasswordPage}?token={Uri.EscapeDataString(tokenResult.Token)}";

        await emailSender.SendAsync(
            delivery.User.Email,
            new ForgotPasswordModel
            {
                Email = delivery.User.Email,
                Name = delivery.User.Name,
                Surname = delivery.User.Surname,
                Link = link,
                DeadlineOfToken = tokenResult.ExpiresInHours
            },
            cancellationToken);
    }

    private async Task<ResetPasswordTokenResult> GetPasswordResetTokenAsync(
        Admin.Data.Entities.Messaging.EmailDelivery delivery,
        IEmailDeliveryRepository repository,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (delivery.PasswordResetToken is not null &&
            delivery.PasswordResetTokenExpiresAt > DateTime.UtcNow)
        {
            return new ResetPasswordTokenResult(
                delivery.PasswordResetToken,
                delivery.PasswordResetTokenExpiresAt.Value);
        }

        var tokenService = serviceProvider.GetRequiredService<IResetPasswordTokenService>();
        var tokenResult = await tokenService.GenerateTokenAsync(delivery.User, cancellationToken);

        await repository.SetPasswordResetTokenAsync(
            delivery.Id,
            tokenResult.Token,
            tokenResult.ExpiresInHours,
            cancellationToken);

        return tokenResult;
    }
}
