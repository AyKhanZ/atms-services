using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Entities.UserProgresses;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Admin.Service.Handlers.UserProgresses;

public class UpdateUserProgressHandler(
    ICurrentUser currentUser,
    IUserProgressRepository userProgressRepository,
    IPasswordHasherService passwordHasherService
    ) :IRequestHandler<UpdateUserProgressCommand>
{
    public async Task Handle(UpdateUserProgressCommand command, CancellationToken cancellationToken)
    {
        var progress = await userProgressRepository.FindAsync(u => u.UserId == currentUser.Id, cancellationToken);
            
        if (progress is null)
        {
            progress = new UserProgress
            {
                UserId = currentUser.Id,
                RoleId = currentUser.RoleId,
                UserProgressType = Enum.Parse<UserProgressTypeEnum>(currentUser.UserType),
                LastUpdated = DateTime.UtcNow
            };
            await userProgressRepository.CreateAsync(progress, cancellationToken);
        }

        if (command.Password is not null)
        {
            progress.PasswordHash = passwordHasherService.Hash(command.Password);
        }
        
        await UpdateInvitedUsersAsync(progress, command.InvitedUsersCommand, cancellationToken);
        await UpdatePersonalInfoAsync(progress, command.PersonalInfoCommand, cancellationToken);
        
        UpdateCurrentStep(progress);
        
        await userProgressRepository.SaveAsync(cancellationToken);
    }

    private async Task UpdatePersonalInfoAsync(UserProgress progress, PersonalInfoCommand? personalInfoCommand, CancellationToken cancellationToken)
    {
        if (personalInfoCommand is null) return;
        
        if (progress.PersonalInfo is null)
        {
            progress.PersonalInfo = new PersonalInfo();
        }
        progress.PersonalInfo.Name = personalInfoCommand.Name;
        progress.PersonalInfo.Email = personalInfoCommand.Email;
        progress.PersonalInfo.Surname = personalInfoCommand.Surname;
        progress.PersonalInfo.PhoneNumber = personalInfoCommand.PhoneNumber;
        progress.PersonalInfo.BirthDate = personalInfoCommand.BirthDate;
        progress.PersonalInfo.AvatarPath = personalInfoCommand.AvatarPath;
        progress.PersonalInfo.Language = personalInfoCommand.Language;
        progress.PersonalInfo.Position = personalInfoCommand.Position;
        progress.PersonalInfo.GenderId = personalInfoCommand.GenderId;
        progress.PersonalInfo.MaritalStatusId = personalInfoCommand.MaritalStatusId;
    }

    private async Task UpdateInvitedUsersAsync(UserProgress userProgress, List<InvitedUsersCommand>? invitedUsersCommand, CancellationToken cancellationToken)
    {
        if (invitedUsersCommand is null) return;
        
        // event send to queue
    }
    
    private void UpdateCurrentStep(UserProgress userProgress)
    {
        userProgress.CurrentStep = userProgress switch
        {
            { PersonalInfo: not null, PasswordHash: not null, InvitedUsers.Count: > 0 } => 3,

            { InvitedUsers.Count: > 0, PasswordHash: not null }
                or { PersonalInfo: not null, PasswordHash: not null }
                or { PersonalInfo: not null, InvitedUsers.Count: > 0 } => 2,

            { PersonalInfo: not null }
                or { PasswordHash: not null }
                or { InvitedUsers.Count: > 0 } => 1,

            _ => 0
        };
    }
}