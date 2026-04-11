using System.Security.Claims;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Security.Models;

namespace ATMS.Admin.Service.Security.Interfaces;

public interface IEmailConfirmationTokenService
{
     EmailConfirmationTokenResult GenerateToken(User user);

     Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
}