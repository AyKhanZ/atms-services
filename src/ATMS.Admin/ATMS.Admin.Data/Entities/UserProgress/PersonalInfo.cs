namespace ATMS.Admin.Data.Entities;

public class PersonalInfo : UserBase
{
    public string Patronymic { get; set; }

    public string Email { get; set; }
    public string PhoneNumber { get; set; }

    public string? PersonalEmail { get; set; }
    public string? PersonalPhoneNumber { get; set; }

    public string Position { get; set; }

    public DateTime BirthDate { get; set; }
    public string Gender { get; set; }

    public string AvatarPath { get; set; }

    public Guid UserProgressId { get; set; }
    public UserProgress UserProgress { get; set; }
}
