using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.WorkProjects;

public class WorkProjectItemModel
{
    public Guid Id { get; set; }

    public string Code { get; set; }

    public string Title { get; set; }

    public WorkProjectOrganizationModel Organization { get; set; }

    public DictionaryModel ProjectType { get; set; }

    public DictionaryModel ProjectKind { get; set; }

    public DictionaryModel ProjectStatus { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }
}
