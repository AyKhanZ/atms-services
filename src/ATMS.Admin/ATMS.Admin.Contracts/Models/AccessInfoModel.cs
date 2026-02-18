namespace ATMS.Admin.Contracts.Models;

public class AccessInfoModel
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime AccessTokenExpireTime { get; set; }
}
