namespace ATMS.Application.Exceptions.Conflict;

public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
