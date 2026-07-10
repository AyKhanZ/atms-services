namespace ATMS.Application.Interfaces;

public interface IDateTimeDisplayService
{
    DateTime ToBakuDateTime(DateTime utcDateTime);
}
