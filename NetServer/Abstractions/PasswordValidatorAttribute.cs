using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class PasswordValidatorAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null) return false; 
        string password = value.ToString()!; 
        
        // k, v rule and error msg
        var rules = new Dictionary<string, string>
        {
            { @".{8,}", "Password must be at least 8 characters long." },
            { @"[A-Z]", "Password must contain at least one uppercase letter." },
            { @"[a-z]", "Password must contain at least one lowercase letter." },
            { @"\d", "Password must contain at least one number." },
            { @"[@$!%*?&]", "Password must contain at least one special character (@, $, !, %, *, ?, &)." }
        };

        foreach (var rule in rules) {
            if (!Regex.IsMatch(password, rule.Key)) 
            {
                ErrorMessage = rule.Value; 
                return false;
            }
        }

        return true; // Password passed all validation checks
    }
}