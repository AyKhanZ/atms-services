using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Data;

namespace ATMS.Admin.Data.Entities.Onboarding;

public class OnboardingPersonalInfo : UserBase
{
    public OnboardingProgress Progress { get; set; }

    public string PhoneNumber { get; set; }

    public string Position { get; set; }

    public int LanguageId { get; set; }

    public Language Language { get; set; }

    public string AvatarPath { get; set; }

    public DateOnly BirthDate { get; set; }

    public int GenderId { get; set; }

    public Gender Gender { get; set; }

    public int MaritalStatusId { get; set; }

    public MaritalStatus MaritalStatus { get; set; }
}
