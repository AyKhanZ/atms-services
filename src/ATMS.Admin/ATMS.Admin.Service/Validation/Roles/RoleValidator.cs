using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Roles;

public class RoleValidator : AbstractValidator<RoleCommand>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    
    public RoleValidator(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ValidationMessages.NameRequired)
            .MaximumLength(30)
            .WithMessage(x => string.Format(ValidationMessages.NameShouldBeLessThan, 30))
            .MustAsync(CheckRoleExistAsync)
            .WithMessage(RoleMessages.AlreadyExists);

        RuleFor(x => x.Description)
            .MaximumLength(100)
            .WithMessage(x => string.Format(ValidationMessages.DescriptionShouldBeLessThan, 100));
        
        RuleFor(x => x.PermissionIds).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(RoleMessages.PermissionsRequired)
            .MustAsync(AllPermissionsExistAsync).WithMessage(RoleMessages.PermissionsNotFound);
    }

    private async Task<bool> AllPermissionsExistAsync(int[] ids, CancellationToken cancellationToken)
    {
        var existingIds = await _permissionRepository.GetExistingIdsAsync(ids, cancellationToken);
        var missingIds = ids.Except(existingIds).ToArray();

        return missingIds.Length == 0;
    }

    private async Task<bool> CheckRoleExistAsync(string name, CancellationToken cancellationToken)
    {
        var result = await _roleRepository.IsExistAsync(r => r.Name == name, cancellationToken);
        return !result;
    }
}
