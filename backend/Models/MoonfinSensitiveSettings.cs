namespace Moonfin.Server.Models;

/// <summary>
/// Per-user setting keys whose values must never appear in logs or diagnostics output.
/// </summary>
public static class MoonfinSensitiveSettings
{
    public static readonly HashSet<string> JsonPropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "userPinHash",
        "jellyseerrApiKey",
        "mdblistApiKey",
        "tmdbApiKey",
    };

    public static bool IsSensitive(string jsonPropertyName) =>
        JsonPropertyNames.Contains(jsonPropertyName);

    /// <summary>Returns a safe placeholder for log output when the key is sensitive.</summary>
    public static string RedactValue(string jsonPropertyName, string? value) =>
        IsSensitive(jsonPropertyName) && !string.IsNullOrEmpty(value) ? "[REDACTED]" : value ?? "(null)";

    /// <summary>Removes sensitive values from a profile before server-side persistence.</summary>
    public static void StripFromProfile(MoonfinSettingsProfile? profile)
    {
        if (profile is null)
        {
            return;
        }

        profile.JellyseerrApiKey = null;
        profile.MdblistApiKey = null;
        profile.TmdbApiKey = null;
        profile.UserPinHash = null;
    }

    /// <summary>Removes sensitive values from user settings and all nested profiles.</summary>
    public static void StripFromUserSettings(MoonfinUserSettings settings)
    {
        settings.JellyseerrApiKey = null;
        settings.MdblistApiKey = null;
        settings.TmdbApiKey = null;
        settings.UserPinHash = null;

        StripFromProfile(settings.Global);
        StripFromProfile(settings.Desktop);
        StripFromProfile(settings.Mobile);
        StripFromProfile(settings.Tv);
    }
}
