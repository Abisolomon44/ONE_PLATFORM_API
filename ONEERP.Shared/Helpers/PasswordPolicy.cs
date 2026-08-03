using System.Text.RegularExpressions;

namespace ONEERP.Shared.Helpers;

/// <summary>
/// Central password policy used by both Platform and ERP APIs.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;

    private static readonly Regex HasUpper = new(@"[A-Z]", RegexOptions.Compiled);
    private static readonly Regex HasLower = new(@"[a-z]", RegexOptions.Compiled);
    private static readonly Regex HasDigit = new(@"\d", RegexOptions.Compiled);
    private static readonly Regex HasSpecial = new(@"[^A-Za-z0-9]", RegexOptions.Compiled);

    public static List<string> Validate(string? password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
            return errors;
        }

        if (password.Length < MinLength)
            errors.Add($"Password must be at least {MinLength} characters long.");

        if (!HasUpper.IsMatch(password))
            errors.Add("Password must contain at least one uppercase letter.");

        if (!HasLower.IsMatch(password))
            errors.Add("Password must contain at least one lowercase letter.");

        if (!HasDigit.IsMatch(password))
            errors.Add("Password must contain at least one digit.");

        if (!HasSpecial.IsMatch(password))
            errors.Add("Password must contain at least one special character.");

        return errors;
    }

    public static bool IsValid(string? password) => Validate(password).Count == 0;
}
