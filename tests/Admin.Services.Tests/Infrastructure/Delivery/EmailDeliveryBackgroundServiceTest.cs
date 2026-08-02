using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Messaging;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure.Delivery;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Security.Models;
using ATMS.Data.Enums;
using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Admin.Services.Tests.Infrastructure.Delivery;

public class EmailDeliveryBackgroundServiceTest
{
    [Fact]
    public async Task ProcessBatchAsync_WhenConfirmationSucceeds_ClearsDeliveryThroughProcessedState()
    {
        var delivery = CreateDelivery();
        var repository = new Mock<IEmailDeliveryRepository>();
        var tokenService = new Mock<IEmailConfirmationTokenService>();
        var emailSender = new Mock<IEmailSender>();
        repository
            .Setup(x => x.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([delivery]);
        repository
            .Setup(x => x.GetAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);
        tokenService
            .Setup(x => x.GenerateToken(delivery.User))
            .Returns(new EmailConfirmationTokenResult("token", DateTime.UtcNow.AddHours(24)));
        emailSender
            .Setup(x => x.SendAsync(
                delivery.User.Email,
                It.Is<InviteModel>(model =>
                    model.Password == "Temporary1!" &&
                    model.Link.Contains("token")),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var worker = CreateWorker(repository.Object, tokenService.Object, emailSender.Object);

        var count = await worker.ProcessOnceAsync(CancellationToken.None);

        Assert.Equal(1, count);
        repository.Verify(
            x => x.MarkProcessedAsync(delivery.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenSmtpFails_SchedulesRetry()
    {
        var delivery = CreateDelivery();
        var repository = new Mock<IEmailDeliveryRepository>();
        var tokenService = new Mock<IEmailConfirmationTokenService>();
        var emailSender = new Mock<IEmailSender>();
        repository
            .Setup(x => x.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([delivery]);
        repository
            .Setup(x => x.GetAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);
        tokenService
            .Setup(x => x.GenerateToken(delivery.User))
            .Returns(new EmailConfirmationTokenResult("token", DateTime.UtcNow.AddHours(24)));
        emailSender
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<InviteModel>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));
        var worker = CreateWorker(repository.Object, tokenService.Object, emailSender.Object);

        await worker.ProcessOnceAsync(CancellationToken.None);

        repository.Verify(
            x => x.MarkRetryAsync(
                delivery.Id,
                1,
                It.Is<DateTime>(date => date > DateTime.UtcNow),
                "SMTP unavailable",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenProcessedStateCannotBeSaved_SchedulesRetry()
    {
        var delivery = CreateDelivery();
        var repository = new Mock<IEmailDeliveryRepository>();
        var tokenService = new Mock<IEmailConfirmationTokenService>();
        var emailSender = new Mock<IEmailSender>();
        repository
            .Setup(x => x.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([delivery]);
        repository
            .Setup(x => x.GetAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);
        repository
            .Setup(x => x.MarkProcessedAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        tokenService
            .Setup(x => x.GenerateToken(delivery.User))
            .Returns(new EmailConfirmationTokenResult("token", DateTime.UtcNow.AddHours(24)));
        emailSender
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<InviteModel>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var worker = CreateWorker(repository.Object, tokenService.Object, emailSender.Object);

        await worker.ProcessOnceAsync(CancellationToken.None);

        repository.Verify(
            x => x.MarkRetryAsync(
                delivery.Id,
                1,
                It.Is<DateTime>(date => date > DateTime.UtcNow),
                "Database unavailable",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenPasswordResetTokenIsStillValid_ReusesIt()
    {
        var delivery = CreatePasswordResetDelivery();
        delivery.PasswordResetToken = "existing-token";
        delivery.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
        var repository = new Mock<IEmailDeliveryRepository>();
        var confirmationTokenService = new Mock<IEmailConfirmationTokenService>();
        var resetTokenService = new Mock<IResetPasswordTokenService>();
        var emailSender = new Mock<IEmailSender>();
        repository
            .Setup(x => x.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([delivery]);
        repository
            .Setup(x => x.GetAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);
        emailSender
            .Setup(x => x.SendAsync(
                delivery.User.Email,
                It.Is<ForgotPasswordModel>(model => model.Link.Contains("existing-token")),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var worker = CreateWorker(
            repository.Object,
            confirmationTokenService.Object,
            emailSender.Object,
            resetTokenService.Object);

        await worker.ProcessOnceAsync(CancellationToken.None);

        resetTokenService.Verify(
            x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        repository.Verify(
            x => x.SetPasswordResetTokenAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenPasswordResetTokenIsMissing_GeneratesAndStoresIt()
    {
        var delivery = CreatePasswordResetDelivery();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var repository = new Mock<IEmailDeliveryRepository>();
        var confirmationTokenService = new Mock<IEmailConfirmationTokenService>();
        var resetTokenService = new Mock<IResetPasswordTokenService>();
        var emailSender = new Mock<IEmailSender>();
        repository
            .Setup(x => x.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([delivery]);
        repository
            .Setup(x => x.GetAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);
        resetTokenService
            .Setup(x => x.GenerateTokenAsync(delivery.User, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResetPasswordTokenResult("new-token", expiresAt));
        emailSender
            .Setup(x => x.SendAsync(
                delivery.User.Email,
                It.Is<ForgotPasswordModel>(model => model.Link.Contains("new-token")),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var worker = CreateWorker(
            repository.Object,
            confirmationTokenService.Object,
            emailSender.Object,
            resetTokenService.Object);

        await worker.ProcessOnceAsync(CancellationToken.None);

        repository.Verify(
            x => x.SetPasswordResetTokenAsync(
                delivery.Id,
                "new-token",
                expiresAt,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenPasswordResetTokenExpired_ReplacesIt()
    {
        var delivery = CreatePasswordResetDelivery();
        delivery.PasswordResetToken = "expired-token";
        delivery.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var repository = new Mock<IEmailDeliveryRepository>();
        var confirmationTokenService = new Mock<IEmailConfirmationTokenService>();
        var resetTokenService = new Mock<IResetPasswordTokenService>();
        var emailSender = new Mock<IEmailSender>();
        repository
            .Setup(x => x.ClaimPendingAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([delivery]);
        repository
            .Setup(x => x.GetAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);
        resetTokenService
            .Setup(x => x.GenerateTokenAsync(delivery.User, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResetPasswordTokenResult("replacement-token", expiresAt));
        emailSender
            .Setup(x => x.SendAsync(
                delivery.User.Email,
                It.Is<ForgotPasswordModel>(model => model.Link.Contains("replacement-token")),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var worker = CreateWorker(
            repository.Object,
            confirmationTokenService.Object,
            emailSender.Object,
            resetTokenService.Object);

        await worker.ProcessOnceAsync(CancellationToken.None);

        repository.Verify(
            x => x.SetPasswordResetTokenAsync(
                delivery.Id,
                "replacement-token",
                expiresAt,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private TestEmailDeliveryBackgroundService CreateWorker(
        IEmailDeliveryRepository repository,
        IEmailConfirmationTokenService tokenService,
        IEmailSender emailSender,
        IResetPasswordTokenService? resetTokenService = null)
    {
        var services = new ServiceCollection()
            .AddSingleton(repository)
            .AddSingleton(tokenService)
            .AddSingleton(resetTokenService ?? Mock.Of<IResetPasswordTokenService>())
            .AddSingleton(emailSender)
            .BuildServiceProvider();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(RedirectUrlOptions)}:{nameof(RedirectUrlOptions.BaseUrl)}"] =
                    "https://localhost:5000/admin/api/v1"
            })
            .Build();

        return new TestEmailDeliveryBackgroundService(
            services.GetRequiredService<IServiceScopeFactory>(),
            new DeliveryRetrySchedule(),
            configuration);
    }

    private EmailDelivery CreateDelivery()
    {
        return new EmailDelivery
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            User = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@baim.az",
                Name = "Aykhan",
                Surname = "Zeynalov"
            },
            Type = EmailDeliveryTypeEnum.Confirmation,
            TemporaryPassword = "Temporary1!",
            Status = DeliveryStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow,
            NextAttemptAt = DateTime.UtcNow
        };
    }

    private EmailDelivery CreatePasswordResetDelivery()
    {
        var delivery = CreateDelivery();
        delivery.Type = EmailDeliveryTypeEnum.PasswordReset;
        delivery.TemporaryPassword = null;
        return delivery;
    }

    private sealed class TestEmailDeliveryBackgroundService(
        IServiceScopeFactory scopeFactory,
        DeliveryRetrySchedule retrySchedule,
        IConfiguration configuration)
        : EmailDeliveryBackgroundService(
            scopeFactory,
            retrySchedule,
            configuration,
            NullLogger<EmailDeliveryBackgroundService>.Instance)
    {
        public Task<int> ProcessOnceAsync(CancellationToken cancellationToken)
        {
            return ProcessBatchAsync(cancellationToken);
        }
    }
}
