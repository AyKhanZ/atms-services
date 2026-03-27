namespace ATMS.Application.Models;

public class ValidationErrorModel
{
    public string Message { get; set; } = "Validation failed";
    public List<FieldError> Errors { get; set; } = [];
}
