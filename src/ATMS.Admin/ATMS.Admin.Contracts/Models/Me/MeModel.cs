namespace ATMS.Admin.Contracts.Models.Me;

public class MeModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string Patronymic { get; set; }

    public string Language { get; set; }
    
    public string AvatarPath { get; set; }
}
