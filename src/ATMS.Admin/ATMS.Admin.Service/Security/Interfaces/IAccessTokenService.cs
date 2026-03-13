using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Security.Models;

namespace ATMS.Admin.Service.Security.Interfaces;

public interface IAccessTokenService
{
    Task<AccessTokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken);
}
