namespace ATMS.Admin.Service.Behaviors.Models;

public class ValidationErrorResponse
{
    public string Message { get; set; } = "Validation failed";
    public List<FieldError> Errors { get; set; } = new();
}