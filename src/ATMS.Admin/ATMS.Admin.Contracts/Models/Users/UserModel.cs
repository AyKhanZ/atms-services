namespace ATMS.Admin.Contracts.Models;

public class UserModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Patronymic { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }

    public string PersonalEmail { get; set; }
    public string PersonalPhoneNumber { get; set; }
    
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; }

    public string AvatarPath { get; set; }
    public string? Position { get; set; }


    public bool HasCompletedSurvey { get; set; }
}