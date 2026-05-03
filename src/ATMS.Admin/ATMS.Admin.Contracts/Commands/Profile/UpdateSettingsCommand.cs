using System.Text.Json.Serialization;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Profile;

public class UpdateSettingsCommand : IRequest
{
    [JsonIgnore]
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required int GenderId { get; set; }
    public required int MaritalStatusId { get; set; }
    public required string Position { get; set; }
    public required string PhoneNumber { get; set; }
    public required DateTime BirthDate { get; set; }
}