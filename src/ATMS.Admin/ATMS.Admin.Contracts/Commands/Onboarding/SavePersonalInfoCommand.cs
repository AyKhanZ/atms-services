using ATMS.Admin.Contracts.Models.Onboarding;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ATMS.Admin.Contracts.Commands.Onboarding;

public class SavePersonalInfoCommand : IRequest<OnboardingModel>
{
    public required string Name { get; set; }
    
    public required string Surname { get; set; }
    
    public required string PhoneNumber { get; set; }
    
    public required string Position { get; set; }
    
    public int LanguageId { get; set; }
    
    public DateOnly BirthDate { get; set; }
    
    public int GenderId { get; set; }
    
    public int MaritalStatusId { get; set; }
    
    public IFormFile? Avatar { get; set; }
    
    public long Version { get; set; }
}
