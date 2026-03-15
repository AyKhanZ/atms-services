using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Exceptions.Entity;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public class ChangePasswordHandler(
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService) : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.Email == command.Email, cancellationToken);
        
        if(user is null) 
            throw new EntityException(EntityErrorType.NotFound, "User not found");
        
        var newPassword = passwordHasherService.Hash(command.NewPassword);

        user.PasswordHash = newPassword;
        
        await userRepository.SaveAsync(cancellationToken);
    }
}