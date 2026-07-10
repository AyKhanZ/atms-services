namespace ATMS.Admin.Contracts.Models.UserProgresses;

public class PersonalInfoModel
{
    public string Name { get; set; }
    
    public string Surname { get; set; }
    
    public string Email { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string Position { get; set; }
    
    public string Language { get; set; }
    
    public string AvatarPath { get; set; }
    
    
    public DateTime BirthDate { get; set; }
    
    
    public int GenderId { get; set; }
    
    public int MaritalStatusId { get; set; }
}