namespace ATMS.Admin.Contracts.Models;

public sealed class AccessInfoModel
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime AccessTokenExpireTime { get; set; }
}
