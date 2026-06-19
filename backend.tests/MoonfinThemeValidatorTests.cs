using System.Text.Json;
using Moonfin.Server.Services;

namespace Moonfin.Server.Tests;

public class MoonfinThemeValidatorTests
{
    private readonly MoonfinThemeValidator _validator = new();

    [Fact]
    public void Validate_rejects_non_object_payload()
    {
        using var doc = JsonDocument.Parse("\"not-an-object\"");

        var result = _validator.Validate(doc.RootElement);

        Assert.False(result.IsValid);
        Assert.Contains("Theme payload must be a JSON object.", result.Errors);
    }

    [Fact]
    public void Validate_rejects_invalid_theme_id()
    {
        var payload = ThemeTestFixtures.MinimalValidTheme(id: "INVALID ID!");

        var result = _validator.Validate(payload);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("id must match"));
    }

    [Fact]
    public void Validate_rejects_script_tags_in_display_name()
    {
        var payload = ThemeTestFixtures.MinimalValidTheme(displayName: "Evil <script>alert(1)</script> Theme");

        var result = _validator.Validate(payload);

        Assert.False(result.IsValid);
        Assert.Contains("displayName cannot contain script tags.", result.Errors);
    }

    [Fact]
    public void Validate_rejects_schema_version_above_one()
    {
        var payload = ThemeTestFixtures.MinimalValidTheme(schemaVersion: 2);

        var result = _validator.Validate(payload);

        Assert.False(result.IsValid);
        Assert.Contains("schemaVersion must be 1 or lower.", result.Errors);
    }

    [Fact]
    public void Validate_rejects_invalid_hex_color()
    {
        var payload = ThemeTestFixtures.MinimalValidTheme(colorOverride: "not-a-color");

        var result = _validator.Validate(payload);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("colors.background"));
    }

    [Fact]
    public void Validate_accepts_minimal_valid_theme()
    {
        var payload = ThemeTestFixtures.MinimalValidTheme();

        var result = _validator.Validate(payload);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal("test-theme", result.ThemeId);
        Assert.Equal("Test Theme", result.DisplayName);
    }
}
