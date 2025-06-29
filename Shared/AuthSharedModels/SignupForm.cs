namespace AuthWithAdmin.Shared.AuthSharedModels;
using System.ComponentModel.DataAnnotations;

public class SignupForm
{
    //משתמש שנרשם לבד
    [Required(ErrorMessage = "שם פרטי הוא שדה חובה")]
    [MinLength(2, ErrorMessage = "שם פרטי חייב להכיל לפחות 2 תווים")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "שם משפחה הוא שדה חובה")]
    [MinLength(2, ErrorMessage = "שם משפחה חייב להכיל לפחות 2 תווים")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "אימייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת אימייל לא תקינה")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "סיסמה היא שדה חובה")]
    [MinLength(6, ErrorMessage = "הסיסמה חייבת להכיל לפחות 6 תווים")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", 
        ErrorMessage = "הסיסמה חייבת להכיל אות גדולה, אות קטנה ומספר")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "אימות סיסמה הוא שדה חובה")]
    [Compare("Password", ErrorMessage = "הסיסמאות אינן תואמות")]
    public string ConfirmPassword { get; set; } = string.Empty;
}