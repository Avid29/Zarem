// Avishai Dernis 2025

using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Zarem.Models.EditorConfig.ColorScheme;
using Zarem.Services;
using Zarem.Services.Settings;
using Zarem.WinUI.Extensions;
using System;
using Windows.UI;

namespace Zarem.WinUI.Controls.CodeEditor;

/// <summary>
/// A collection of colors to use for syntax highlighting.
/// </summary>
public class AssemblySyntaxHighlightingTheme : DependencyObject
{
    public event EventHandler? Updated;

    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="BackgroundColor"/> property.
    /// </summary>
    public static readonly DependencyProperty BackgroundColorProperty =
        DependencyProperty.Register(nameof(BackgroundColor),
            typeof(Color),
            typeof(AssemblySyntaxHighlightingTheme),
            new PropertyMetadata(Colors.Transparent));

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblySyntaxHighlightingTheme"/> class.
    /// </summary>
    public AssemblySyntaxHighlightingTheme() : this(CurrentScheme)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblySyntaxHighlightingTheme"/> class.
    /// </summary>
    public AssemblySyntaxHighlightingTheme(EditorColorScheme? scheme)
    {
        if (scheme is not null)
        {
            LoadFromScheme(scheme);
        }
    }

    /// <summary>
    /// Gets or sets the base color to use when alpha blending a syntax highlight color.
    /// </summary>
    public Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color to use for highlighting instruction tokens.
    /// </summary>
    public Color InstructionHighlightColor { get; private set; }

    /// <summary>
    /// Gets or sets the color to use for highlighting register tokens.
    /// </summary>
    public Color RegisterHighlightColor { get; private set; }

    /// <summary>
    /// Gets or sets the color to use for highlighting immediate value tokens.
    /// </summary>
    public Color ImmediateHighlightColor { get; private set; }

    /// <summary>
    /// Gets or sets the color to use for highlighting reference tokens.
    /// </summary>
    public Color ReferenceHighlightColor { get; private set; }

    /// <summary>
    /// Gets or sets the color to use for highlighting operator tokens.
    /// </summary>
    public Color OperatorHighlightColor { get; private set; }

    /// <summary>
    /// Gets or sets the color to use for highlighting directive tokens.
    /// </summary>
    public Color DirectiveHighlightColor { get; private set; }

    /// <summary>
    /// Gets or sets the color to use for highlighting string tokens.
    /// </summary>
    public Color StringHighlightColor { get; private set; }

    /// <summary>
    /// Gets or sets the color to use for highlighting comment tokens.
    /// </summary>
    public Color CommentHighlightColor { get; private set; }

    /// <summary>
    /// Gets or sets the color to use for highlighting macro tokens.
    /// </summary>
    public Color MacroHighlightColor { get; private set; }

    /// <summary>
    /// Gets the color to use for highlighting invalid instruction names.
    /// </summary>
    public Color InvalidInstructionHighlightColor => ApplyBackgroundBlend(InstructionHighlightColor, 0.6);

    /// <summary>
    /// Gets the color to use for highlighting invalid references.
    /// </summary>
    public Color InvalidReferenceHighlightColor => ApplyBackgroundBlend(ReferenceHighlightColor, 0.6);

    /// <summary>
    /// Gets or sets the color to use for underlining errors.
    /// </summary>
    public Color ErrorUnderlineColor { get; private set; }

    /// <summary>
    /// Gets or sets the color to use for underlining warnings.
    /// </summary>
    public Color WarningUnderlineColor { get; private set; }

    /// <summary>
    /// Gets or sets the color to use for underlining messages.
    /// </summary>
    public Color MessageUnderlineColor { get; private set; }

    public static AssemblySyntaxHighlightingTheme Current => new(CurrentScheme);

    private static EditorColorScheme? CurrentScheme
        => Service.Get<ISettingsService>().Local.GetValue<EditorColorScheme>(SettingsKeys.EditorColorScheme);

    public void ReloadFromSettings()
    {
        if (CurrentScheme is null)
            return;

        LoadFromScheme(CurrentScheme);
    }

    public void LoadFromScheme(EditorColorScheme scheme)
    {
        BackgroundColor = scheme.Background.ToColor();

        var syntax = scheme.SyntaxHighlighting;
        InstructionHighlightColor = syntax.Instruction.ToColor();
        RegisterHighlightColor = syntax.Register.ToColor();
        ImmediateHighlightColor = syntax.Immediate.ToColor();
        ReferenceHighlightColor = syntax.Reference.ToColor();
        OperatorHighlightColor = syntax.Operator.ToColor();
        DirectiveHighlightColor = syntax.Directive.ToColor();
        StringHighlightColor = syntax.String.ToColor();
        CommentHighlightColor = syntax.Comment.ToColor();
        MacroHighlightColor = syntax.Macro.ToColor();

        var logColors = scheme.LogColors;
        ErrorUnderlineColor = logColors.Error.ToColor();
        WarningUnderlineColor = logColors.Warning.ToColor();
        MessageUnderlineColor = logColors.Message.ToColor();

        Updated?.Invoke(this, EventArgs.Empty);
    }

    private Color ApplyBackgroundBlend(Color overlay, double opacity)
    {
        overlay.A = (byte)(overlay.A * opacity);
        var result = BackgroundColor.AlphaBlend(overlay);
        result.A = 255;
        return result;
    }
}
