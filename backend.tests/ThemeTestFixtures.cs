using System.Text.Json;

namespace Moonfin.Server.Tests;

internal static class ThemeTestFixtures
{
    private static readonly string[] ColorKeys =
    [
        "background", "onBackground", "surface", "onSurface", "surfaceVariant", "scrim",
        "accent", "onAccent", "buttonNormal", "buttonFocused", "buttonDisabled", "buttonActive",
        "onButtonNormal", "onButtonFocused", "onButtonDisabled", "inputBackground", "inputFocused",
        "inputBorder", "inputBorderFocused", "rangeTrack", "rangeProgress", "rangeThumb", "seekbarBuffered",
        "badgeBackground", "onBadge", "badgeUnplayed", "badgeWatched", "recordingActive", "recordingScheduled"
    ];

    private static readonly string[] SemanticKeys =
    [
        "statusAvailable", "statusRequested", "statusPending", "statusDownloading",
        "mediaTypeBadgeMovie", "mediaTypeBadgeShow"
    ];

    private static readonly string[] BookColorKeys =
    [
        "background", "accent", "mutedText", "primaryText", "sectionTitle", "divider",
        "placeholder", "shadow", "gradientTop", "gradientBottom", "inactiveChip"
    ];

    public static JsonElement MinimalValidTheme(
        string id = "test-theme",
        string displayName = "Test Theme",
        int schemaVersion = 1,
        string? colorOverride = null)
    {
        var color = colorOverride ?? "#112233";
        var colors = new Dictionary<string, string>();
        foreach (var key in ColorKeys)
        {
            colors[key] = color;
        }

        var semantic = new Dictionary<string, string>();
        foreach (var key in SemanticKeys)
        {
            semantic[key] = color;
        }

        var book = new Dictionary<string, object>();
        foreach (var key in BookColorKeys)
        {
            book[key] = color;
        }

        book["placeholderPalette"] = new[] { color };

        var border = new Dictionary<string, object>
        {
            ["color"] = color,
            ["width"] = 1,
        };

        var payload = new Dictionary<string, object?>
        {
            ["schemaVersion"] = schemaVersion,
            ["id"] = id,
            ["displayName"] = displayName,
            ["colors"] = colors,
            ["semantic"] = semantic,
            ["book"] = book,
            ["borders"] = new Dictionary<string, object?>
            {
                ["cardBorder"] = border,
                ["chipBorder"] = border,
                ["focusBorder"] = border,
                ["cardRadius"] = 4,
                ["chipRadius"] = 4,
                ["chipBackground"] = color,
                ["focusGlow"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["color"] = color,
                        ["blurRadius"] = 4,
                        ["offsetX"] = 0,
                        ["offsetY"] = 0,
                        ["spreadRadius"] = 0,
                    }
                },
            },
        };

        var json = JsonSerializer.Serialize(payload);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
