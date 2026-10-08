namespace ATMS.Project.Contracts.Models.WorkProjects;

public sealed class WorkProjectOrganizationModel
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string? LogoPath { get; set; }
}
