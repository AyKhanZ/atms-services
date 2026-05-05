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
                OrganizationId = currentUser.OrganizationId,
                UserProgressType = Enum.Parse<UserProgressTypeEnum>(currentUser.UserType),
                LastUpdated = DateTime.UtcNow
            };
            await userProgressRepository.CreateAsync(progress, cancellationToken);
        }

        if (command.Password is not null)
        {
            progress.PasswordHash = passwordHasherService.Hash(command.Password);
        }
        
        UpdateInvitedUsers(progress, command.InvitedUsersCommand);
        UpdatePersonalInfo(progress, command.PersonalInfoCommand);
        
        UpdateCurrentStep(progress);
        
        await userProgressRepository.SaveAsync(cancellationToken);
    }

    private void UpdatePersonalInfo(UserProgress progress, PersonalInfoCommand? personalInfoCommand)
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

    private void UpdateInvitedUsers(UserProgress userProgress, List<InvitedUsersCommand>? invitedUsersCommand)
    {
        if (invitedUsersCommand is null) return;
        
        userProgress.InvitedUsers?.Clear();
        
        userProgress.InvitedUsers = invitedUsersCommand.Select(invitedUser => new InvitedUser
        {
            Id = Guid.NewGuid(),
            Name = invitedUser.Name,
            Surname = invitedUser.Surname,
            Email = invitedUser.Email,
            UserProgressId = userProgress.UserId
        }).ToList();
    }
    
    private void UpdateCurrentStep(UserProgress userProgress)
    {
        var hasPersonalInfo = userProgress.PersonalInfo is not null;
        var hasPassword = userProgress.PasswordHash is not null;
        var hasInvitedUsers = userProgress.InvitedUsers?.Count > 0;

        var isClientManager = userProgress.UserProgressType == UserProgressTypeEnum.ClientManager;

        // ClientManager — 3/3 steps
        if (isClientManager)
        {
            var steps = 0;

            if (hasPersonalInfo) steps++;
            if (hasPassword) steps++;
            if (hasInvitedUsers) steps++;

            userProgress.CurrentStep = (ushort)steps;
            return;
        }

        // Client / Agent
        if (hasPersonalInfo && hasPassword)
        {
            userProgress.CurrentStep = 2;
        }
        // Anyone — 1 step
        else if (hasPersonalInfo || hasPassword)
        {
            userProgress.CurrentStep = 1;
        }
        else
        {
            userProgress.CurrentStep = 0;
        }
    }
}