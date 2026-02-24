// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using WinUIEditor;

namespace Zarem.WinUI.Controls.CodeEditor;

/// <summary>
/// A wrapper of the <see cref="CodeEditorControl"/> to handle Zarem events.
/// </summary>
public partial class CodeEditor : Control
{
    private const string CodeEditorPartName = "PART_CodeEditorControl";

    /// <summary>
    /// An event invoked when the <see cref="Text"/> property changes
    /// </summary>
    public event EventHandler? TextChanged;

    public CodeEditor()
    {
        DefaultStyleKey = typeof(CodeEditor);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Setup template parts


        // Setup events
        this.Loaded += CodeEditor_Loaded;

        // Setup keybinds
        SetupKeybinds();

        // Apply the current text
        UpdateText();
    }
}
