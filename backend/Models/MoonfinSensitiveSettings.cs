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
}
