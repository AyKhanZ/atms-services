using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Contracts.Requests.Profile;
using ATMS.Application.Security;

namespace Admin.Services.Tests.Contracts;

public class PersonalSettingsRequestsTest
{
    [Theory]
    [InlineData(typeof(GetProfileRequest))]
    [InlineData(typeof(UpdateSettingsCommand))]
    [InlineData(typeof(UpdateLanguageCommand))]
    [InlineData(typeof(ChangePasswordCommand))]
    public void PersonalSettingsRequests_ExcludeSuperAdmin(Type requestType)
    {
        Assert.True(requestType.IsDefined(typeof(ExceptSuperAdminAccessAttribute), false));
    }
}
