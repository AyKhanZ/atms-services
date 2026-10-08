using System.ComponentModel.DataAnnotations;

namespace ATMS.Infrastructure.Options;

public sealed class AdminOptions
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Surname { get; init; }

    [Required]
    public required string RoleName { get; init; }

    [Required]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
