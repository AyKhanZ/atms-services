using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Users;

public class UpdateUserStatusValidator: AbstractValidator<UpdateUserStatusCommand>
{
    private readonly IDictionariesRepository _dictionariesRepository;
    private readonly IUserRepository _userRepository;

    public UpdateUserStatusValidator(IDictionariesRepository dictionariesRepository, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _dictionariesRepository = dictionariesRepository;
        
        RuleFor(s => s.Id).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Id is required.")
            .MustAsync(IsUserExistAsync).WithMessage("User not does not exist.");
        
        RuleFor(s => s.UserStatusId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("User status is required.")
            .MustAsync(IsUserStatusExistAsync).WithMessage("User status does not exist.");
    }
    
    private Task<bool> IsUserExistAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _userRepository.IsExistAsync(u => u.Id == userId, cancellationToken);
    }
    
    private Task<bool> IsUserStatusExistAsync(int userStatusId, CancellationToken cancellationToken)
    {
        return _dictionariesRepository.IsUserStatusExistAsync(m => m.Id == userStatusId, cancellationToken);
    }
}