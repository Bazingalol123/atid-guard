namespace AuthWithAdmin.Shared.AuthSharedModels;
using System.ComponentModel.DataAnnotations;

public class LoginForm
{
    [Required(ErrorMessage = "אימייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת אימייל לא תקינה")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "סיסמה היא שדה חובה")]
    [MinLength(6, ErrorMessage = "הסיסמה חייבת להכיל לפחות 6 תווים")]
    public string Password { get; set; } = string.Empty;
}