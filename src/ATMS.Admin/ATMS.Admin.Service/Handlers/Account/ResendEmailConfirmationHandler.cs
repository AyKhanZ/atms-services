using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using ATMS.Exceptions.Configuration;
using ATMS.Exceptions.Entity;
using ATMS.Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Handlers.Account;

public class ResendEmailConfirmationHandler(
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService,
    IPasswordService passwordService,
    IEmailConfirmationTokenService emailConfirmationTokenService,
    IEmailSender emailSender,
    IConfiguration configuration
    ) : IRequestHandler<ResendEmailConfirmationCommand>
{
    private readonly RedirectUrlOptions _redirectUrlOptions =
        configuration.GetSection(nameof(RedirectUrlOptions)).Get<RedirectUrlOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.RedirectUrlSectionNotFound,
                $"Configuration for section '{nameof(RedirectUrlOptions)}' is not found or could not be loaded.");

    public async Task Handle(ResendEmailConfirmationCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(
            u => u.Email == command.Email,
            cancellationToken);

        if (user is null)
        {
            throw new EntityException(EntityErrorType.NotFound, "User not found .");
        }

        var emailConfirmationTokenResult = emailConfirmationTokenService.GenerateToken(user);
        var link = GenerateConfirmationLink(emailConfirmationTokenResult.Token);

        var rndPassword = passwordService.GenerateRandomPassword();
        user.PasswordHash = passwordHasherService.Hash(rndPassword);

        await emailSender.SendAsync(user.Email,
            new InviteModel
            {
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                Password = rndPassword,
                Link = link,
                DeadlineOfToken = emailConfirmationTokenResult.ExpiresInHours
            }, cancellationToken);
    }

    private string GenerateConfirmationLink(string token) =>
        $"{_redirectUrlOptions.BaseUrl}/account/confirm?token={Uri.EscapeDataString(token)}";
}
