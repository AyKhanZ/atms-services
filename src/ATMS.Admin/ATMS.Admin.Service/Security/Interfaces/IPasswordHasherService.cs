namespace ATMS.Admin.Service.Security.Interfaces;

public interface IPasswordHasherService
{
    public string Hash(string password);
    public bool Verify(string password, string passwordHash);
}
