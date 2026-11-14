// Avishai Dernis 2025

using System.IO;
using System.Reflection;
using System.Text.Json;
using Zarem.IDE.Models.EditorConfig.ColorScheme;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.Services;
using Zarem.Services.Settings;

namespace Zarem.IDE;

/// <summary>
/// A collection of keys used for settings.
/// </summary>
public static class SettingsKeys
{
#pragma warning disable CS1591
    public const string RestoreOpenProject = nameof(RestoreOpenProject);  // Old setting, key not removed

    // Editor
    public const string RealTimeAssembly = nameof(RealTimeAssembly);
    public const string AnnotationThreshold = nameof(AnnotationThreshold);
    public const string EditorColorSchemeBase = nameof(EditorColorSchemeBase);

    public static string EditorColorScheme => $"{EditorColorSchemeBase}-{Zarem.SettingsKeys.CurrentTheme}";

    // Assembler
    public const string AssemblerLanguageOverride = nameof(AssemblerLanguageOverride);

    public static EditorColorScheme? DefaultDarkColorScheme => LoadEditorColorScheme("Zarem-Dark.json");
    public static EditorColorScheme? DefaultLightColorScheme => LoadEditorColorScheme("Zarem-Light.json");
#pragma warning restore CS1591

    private static EditorColorScheme? LoadEditorColorScheme(string resource)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Load
        using Stream? stream = assembly.GetManifestResourceStream($"Zarem.IDE.Resources.ColorSchemes.{resource}");
        if (stream is null)
            return null;

        // Deserialize
        var editorColorScheme = JsonSerializer.Deserialize<EditorColorScheme>(stream);
        if (editorColorScheme is null)
            return null;

        return editorColorScheme;
    }
}
