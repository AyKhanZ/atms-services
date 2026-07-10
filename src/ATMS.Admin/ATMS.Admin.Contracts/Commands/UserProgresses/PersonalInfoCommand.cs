namespace ATMS.Admin.Contracts.Commands.UserProgresses;

public class PersonalInfoCommand
{
    public required string Name { get; set; }
    
    public required string Surname { get; set; }
    
    public required string Email { get; set; }
    
    public required string PhoneNumber { get; set; }
    
    public required string Position { get; set; }
    
    public required string Language { get; set; }
    
    public required string AvatarPath { get; set; }
    
    
    public required DateTime BirthDate { get; set; }
    
    
    public required int GenderId { get; set; }
    
    public required int MaritalStatusId { get; set; }
}