namespace ATMS.Project.Contracts.Models.WorkTickets;

public class WorkTicketAssigneeModel
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string? AvatarPath { get; set; }
}
