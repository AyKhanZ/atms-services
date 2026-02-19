using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Interfaces;
using ATMS.Exceptions.Entity;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public class RegisterHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IMapper mapper)
    : IRequestHandler<RegisterCommand, UserModel>
{
    public async Task<UserModel> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<User>(command);
        entity.Id = Guid.NewGuid();

        var role = await roleRepository.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            throw new EntityException(EntityErrorType.EntityNotFound, "Role not found");
        }

        var userRole = new UserRole
        {
            UserId = entity.Id,
            RoleId = role.Id
        };
        entity.UserRoles = [userRole];

        await userRepository.CreateAsync(entity, cancellationToken);

        return mapper.Map<UserModel>(entity);
    }
}
