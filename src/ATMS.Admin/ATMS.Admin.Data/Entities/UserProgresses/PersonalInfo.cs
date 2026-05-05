using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Data;

namespace ATMS.Admin.Data.Entities.UserProgresses;

public class PersonalInfo : UserBase
{
    public string PhoneNumber { get; set; }
    
    public string Position { get; set; }
    
    public string Language { get; set; }
    
    public string AvatarPath { get; set; }
    
    public DateTime BirthDate { get; set; }


    public Guid UserProgressId { get; set; }
    
    
    public int GenderId { get; set; }

    public Gender Gender { get; set; }
    
    
    public int MaritalStatusId { get; set; }

    public MaritalStatus MaritalStatus { get; set; }
}