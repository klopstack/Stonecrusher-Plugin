using Moonfin.Server.Models;

namespace Moonfin.Server.Tests;

public class MoonfinSensitiveSettingsTests
{
    [Theory]
    [InlineData("userPinHash")]
    [InlineData("jellyseerrApiKey")]
    [InlineData("mdblistApiKey")]
    [InlineData("tmdbApiKey")]
    [InlineData("JELLYSEERRAPIKEY")]
    public void IsSensitive_recognizes_known_keys(string key)
    {
        Assert.True(MoonfinSensitiveSettings.IsSensitive(key));
    }

    [Theory]
    [InlineData("visualTheme")]
    [InlineData("navbarPosition")]
    [InlineData("")]
    public void IsSensitive_ignores_non_sensitive_keys(string key)
    {
        Assert.False(MoonfinSensitiveSettings.IsSensitive(key));
    }

    [Fact]
    public void RedactValue_masks_sensitive_non_empty_values()
    {
        Assert.Equal("[REDACTED]", MoonfinSensitiveSettings.RedactValue("jellyseerrApiKey", "secret-key"));
    }

    [Theory]
    [InlineData("jellyseerrApiKey", null, "(null)")]
    [InlineData("jellyseerrApiKey", "", "")]
    [InlineData("visualTheme", "dark", "dark")]
    [InlineData("visualTheme", null, "(null)")]
    public void RedactValue_leaves_safe_values_unchanged(string key, string? value, string expected)
    {
        Assert.Equal(expected, MoonfinSensitiveSettings.RedactValue(key, value));
    }

    [Fact]
    public void StripFromProfile_clears_all_sensitive_profile_fields()
    {
        var profile = new MoonfinSettingsProfile
        {
            JellyseerrApiKey = "jelly-key",
            MdblistApiKey = "mdb-key",
            TmdbApiKey = "tmdb-key",
            UserPinHash = "pin-hash",
            VisualTheme = "dark",
        };

        MoonfinSensitiveSettings.StripFromProfile(profile);

        Assert.Null(profile.JellyseerrApiKey);
        Assert.Null(profile.MdblistApiKey);
        Assert.Null(profile.TmdbApiKey);
        Assert.Null(profile.UserPinHash);
        Assert.Equal("dark", profile.VisualTheme);
    }

    [Fact]
    public void StripFromUserSettings_clears_legacy_and_nested_profile_fields()
    {
        var settings = new MoonfinUserSettings
        {
            JellyseerrApiKey = "legacy-jelly",
            Global = new MoonfinSettingsProfile { MdblistApiKey = "global-mdb" },
            Tv = new MoonfinSettingsProfile { TmdbApiKey = "tv-tmdb" },
        };

        MoonfinSensitiveSettings.StripFromUserSettings(settings);

        Assert.Null(settings.JellyseerrApiKey);
        Assert.Null(settings.Global!.MdblistApiKey);
        Assert.Null(settings.Tv!.TmdbApiKey);
    }

    [Fact]
    public void StripFromUserSettings_preserves_non_sensitive_fields_on_ingest()
    {
        var settings = new MoonfinUserSettings
        {
            Global = new MoonfinSettingsProfile
            {
                JellyseerrApiKey = "must-strip",
                JellyseerrEnabled = true,
            },
        };

        MoonfinSensitiveSettings.StripFromUserSettings(settings);

        Assert.Null(settings.Global!.JellyseerrApiKey);
        Assert.True(settings.Global.JellyseerrEnabled);
    }
}
