using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Roles;

public class RoleValidator : AbstractValidator<RoleCommand>
{
    private readonly IRoleRepository _roleRepository;
    public RoleValidator(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
        
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Role name is required.")
            .MaximumLength(20)
            .WithMessage("Role name must not exceed 20 characters.")
            .MustAsync(CheckRoleExistAsync)
            .WithMessage("Role with this name already exists.");

        RuleFor(x => x.Description)
            .MaximumLength(100)
            .WithMessage("Role description must not exceed 100 characters.");
    }

    private async Task<bool> CheckRoleExistAsync(string name, CancellationToken cancellationToken)
    {
        var result = await _roleRepository.IsExistAsync(r => r.Name == name, cancellationToken);
        return !result;
    }
}
