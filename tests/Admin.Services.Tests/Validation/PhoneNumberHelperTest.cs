using ATMS.Application.Dispatcher.Validation;

namespace Admin.Services.Tests.Validation;

public sealed class PhoneNumberHelperTest
{
    [Theory]
    [InlineData("+994501234567")]
    [InlineData("+447911123456")]
    public void IsValid_WhenPhoneNumberIsInternational_ReturnsTrue(string phoneNumber)
    {
        var result = PhoneNumberHelper.IsValid(phoneNumber);

        Assert.True(result);
    }

    [Theory]
    [InlineData("not-a-phone")]
    [InlineData("0501234567")]
    public void IsValid_WhenPhoneNumberIsInvalid_ReturnsFalse(string phoneNumber)
    {
        var result = PhoneNumberHelper.IsValid(phoneNumber);

        Assert.False(result);
    }
}
