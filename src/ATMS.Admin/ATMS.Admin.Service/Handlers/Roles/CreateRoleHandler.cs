using ATMS.Admin.Contracts.Commands;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, RoleModel>
{
    private readonly IMapper _mapper;
    private readonly IRoleRepository _roleRepository;

    public CreateRoleHandler(IMapper mapper,
        IRoleRepository roleRepository)
    {
        _mapper = mapper;
        _roleRepository = roleRepository;
    }

    public async Task<RoleModel> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Role>(command);
        entity.Id = Guid.NewGuid();

        await _roleRepository.CreateAsync(entity, cancellationToken);

        return _mapper.Map<RoleModel>(entity);
    }
}
