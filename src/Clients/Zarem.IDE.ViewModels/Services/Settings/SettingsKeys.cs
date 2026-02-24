// Avishai Dernis 2025

using CommunityToolkit.Mvvm.DependencyInjection;
using Zarem.Models.EditorConfig.ColorScheme;
using Zarem.Services.Settings.Enums;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Zarem.Services.Settings;

/// <summary>
/// A collection of keys used for settings.
/// </summary>
public static class SettingsKeys
{
#pragma warning disable CS1591
    // App
    public const string AppTheme = "AppTheme";
    public const string LanguageOverride = "LanguageOverride";
    public const string RestoreOpenProject = "RestoreOpenProject";  // Old setting, key not removed

    // Editor
    public const string RealTimeAssembly = "RealTimeAssembly";
    public const string AnnotationThreshold = "AnnotationThreshold";
    public const string EditorColorSchemeBase = "EditorColorScheme";

    public static string EditorColorScheme => $"{EditorColorSchemeBase}-{CurrentTheme}";

    // Assembler
    public const string AssemblerLanguageOverride = "AssemblerLanguageOverride";

    public static EditorColorScheme? DefaultDarkColorScheme => LoadEditorColorScheme("Zarem-Dark.json");
    public static EditorColorScheme? DefaultLightColorScheme => LoadEditorColorScheme("Zarem-Light.json");

    private static string CurrentTheme
    {
        get
        {
            var theme = SettingsService.Local.GetValue<Theme>(AppTheme);
            if (theme is Theme.Default)
            {
                theme = SettingsService.DefaultTheme;
            }

            return theme switch
            {
                Theme.Light => "Light",
                Theme.Dark or _ => "Dark",
            };
        }
    }

    private static ISettingsService SettingsService => Service.Get<ISettingsService>();

    private static EditorColorScheme? LoadEditorColorScheme(string resource)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Load
        using Stream? stream = assembly.GetManifestResourceStream($"Zarem.Resources.ColorSchemes.{resource}");
        if (stream is null)
            return null;

        // Deserialize
        var editorColorScheme = JsonSerializer.Deserialize<EditorColorScheme>(stream);
        if (editorColorScheme is null)
            return null;

        return editorColorScheme;
    }
#pragma warning restore CS1591
}
