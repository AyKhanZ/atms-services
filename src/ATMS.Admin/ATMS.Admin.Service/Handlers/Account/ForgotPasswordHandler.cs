using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Handlers.Account;

public class ForgotPasswordHandler(
    IUserRepository userRepository,
    IEmailSender emailSender,
    IResetPasswordTokenService resetPasswordTokenService,
    IConfiguration configuration) : IRequestHandler<ForgotPasswordCommand>
{
    
    private readonly RedirectUrlOptions  _redirectUrlOptions =
        configuration.GetSection(nameof(RedirectUrlOptions)).Get<RedirectUrlOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.RedirectUrlSectionNotFound,
                string.Format(ExceptionMessages.ConfigSectionNotFound, nameof(RedirectUrlOptions)));

    public async Task Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.Email == command.Email, cancellationToken);
        if (user is null)
        {
            throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
        }
        
        var tokenResult = await resetPasswordTokenService.GenerateTokenAsync(user, cancellationToken);

        var link = GenerateResetPasswordUrl(tokenResult.Token);
        
        await emailSender.SendAsync(user.Email,
            new ForgotPasswordModel
            {
                Link = link,
                DeadlineOfToken = tokenResult.ExpiresInHours
            }, cancellationToken);
    }
    
    private string GenerateResetPasswordUrl(string resetToken) =>
        $"{_redirectUrlOptions.ResetPasswordPage}?token={Uri.EscapeDataString(resetToken)}";
}
