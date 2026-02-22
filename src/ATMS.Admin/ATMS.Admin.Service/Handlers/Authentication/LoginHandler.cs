using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Exceptions.Entity;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public class LoginHandler(
    IUserRepository userRepository,
    ITokenService tokenService) : IRequestHandler<LoginCommand, AccessInfoModel>
{
    public async Task<AccessInfoModel> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByEmail(command.Email, cancellationToken);
        if (user is null)
        {
            throw new EntityException(EntityErrorType.NotFound, $"User with email: {command.Email} not found. ");
        }

        var token = await tokenService.GenerateTokenAsync(user, cancellationToken);

        return new AccessInfoModel
        {
            AccessToken = token,
            RefreshToken = "token.RefreshToken",
            AccessTokenExpireTime = new DateTime()
        };
    }
}
