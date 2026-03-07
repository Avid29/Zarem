// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using WinUIEditor;

namespace Zarem.IDE.Controls.CodeEditor;

/// <summary>
/// A wrapper of the <see cref="CodeEditorControl"/> to handle Zarem events.
/// </summary>
[TemplatePart(Name = CodeEditorPartName, Type = typeof(CodeEditorControl))]
public partial class CodeEditor : Control
{
    private const string CodeEditorPartName = "PART_CodeEditorControl";
    private const int BaseFontSize = 10;

    /// <summary>
    /// An event invoked when the <see cref="Text"/> property changes
    /// </summary>
    public event EventHandler? TextChanged;

    protected CodeEditorControl? ChildEditor { get; private set; }

    public CodeEditor()
    {
        DefaultStyleKey = typeof(CodeEditor);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Setup template parts
        ChildEditor = (CodeEditorControl)GetTemplateChild(CodeEditorPartName);

        // Setup events
        this.Loaded += CodeEditor_Loaded;

        // Setup keybinds
        SetupKeybinds();

        // Apply the current text
        UpdateText(true);
    }

    [MemberNotNullWhen(true, nameof(ChildEditor))]
    protected bool TryGetEditor([NotNullWhen(true)] out Editor? editor)
    {
        editor = ChildEditor?.Editor;
        return editor is not null;
    }

    private static int ZoomPercentageToFactor(int baseSize, int percentage)
    {
        double size = (percentage * baseSize) / 100d;
        int factor = (int)Math.Round(size - baseSize);
        return Math.Clamp(factor, -10, 20);
    }

    private static int ZoomFactorToPercentage(int baseSize, int factor)
    {
        double percentage = ((double)(baseSize + factor) / baseSize) * 100;
        return (int)Math.Round(percentage);
    }
}
