namespace ATMS.Swagger.Constants;

public static class SwaggerConstants
{
    public const string ApiVersion = "v1";

    public static string GetDescription(string title)
        => $"Swagger for ATMS {title} documentation";
}
