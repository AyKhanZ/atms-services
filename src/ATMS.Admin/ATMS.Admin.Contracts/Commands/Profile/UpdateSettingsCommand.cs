using System.Text.Json.Serialization;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Profile;

public class UpdateSettingsCommand : IRequest
{
    [JsonIgnore]
    public required Guid Id { get; set; }
    public required string Name { get; init; }
    public required string Surname { get; init; }
    public required int GenderId { get; init; }
    public required int MaritalStatusId { get; init; }
    public required string Position { get; init; }
    public required string PhoneNumber { get; init; }
    public required DateTime BirthDate { get; init; }
}