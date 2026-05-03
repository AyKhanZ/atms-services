using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Users;

public class UpdateUserStatusValidator: AbstractValidator<UpdateUserStatusCommand>
{
    private readonly IDictionariesRepository _dictionariesRepository;

    public UpdateUserStatusValidator(IDictionariesRepository dictionariesRepository)
    {
        _dictionariesRepository = dictionariesRepository;
        
        RuleFor(s => s.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);
        
        RuleFor(s => s.UserStatusId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.UserStatusRequired)
            .MustAsync(IsUserStatusExistAsync).WithMessage(ProfileMessages.UserStatusNotSupported);
    }
    
    private Task<bool> IsUserStatusExistAsync(int userStatusId, CancellationToken cancellationToken)
    {
        return _dictionariesRepository.IsUserStatusExistAsync(m => m.Id == userStatusId, cancellationToken);
    }
}