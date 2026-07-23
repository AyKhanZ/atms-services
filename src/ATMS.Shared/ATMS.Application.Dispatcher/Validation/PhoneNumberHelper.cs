using PhoneNumbers;

namespace ATMS.Application.Dispatcher.Validation;

public static class PhoneNumberHelper
{
    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

    public static bool IsValid(string phoneNumber)
    {
        try
        {
            return PhoneUtil.IsValidNumber(PhoneUtil.Parse(phoneNumber, null));
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
