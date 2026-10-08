namespace ATMS.Application.Exceptions.Conflict;

public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
