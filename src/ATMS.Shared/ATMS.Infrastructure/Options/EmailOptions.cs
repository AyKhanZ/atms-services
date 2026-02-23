namespace ATMS.Infrastructure.Options;

public class EmailOptions
{
    public required string From { get; init; }
    public required string SmtpServer { get; init; }
    public required int Port { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
}
