namespace ATMS.Admin.Contracts.Models.Profile;

public sealed class ProfileModel
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Position { get; set; }
    public int LanguageId { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int? GenderId { get; set; }
    public int? MaritalStatusId { get; set; }
    public string AvatarPath { get; set; }
}
