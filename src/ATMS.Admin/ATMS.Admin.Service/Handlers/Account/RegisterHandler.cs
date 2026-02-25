using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public class RegisterHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IMapper mapper,
    IPasswordService passwordService,
    IPasswordHasherService passwordHasherService)
    : IRequestHandler<RegisterCommand, UserModel>
{
    public async Task<UserModel> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<User>(command);
        entity.Id = Guid.NewGuid();

        var role = await roleRepository.GetByIdAsync(command.RoleId, cancellationToken);

        var userRole = new UserRole
        {
            UserId = entity.Id,
            RoleId = role.Id
        };
        entity.UserRoles = [userRole];

        var rndPassword = passwordService.GenerateRandomPassword();
        entity.PasswordHash = passwordHasherService.Hash(rndPassword);

        await userRepository.CreateAsync(entity, cancellationToken);

        return mapper.Map<UserModel>(entity);
    }
}
