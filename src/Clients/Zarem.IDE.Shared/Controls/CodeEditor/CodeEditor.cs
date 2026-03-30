// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor;

[TemplatePart(Name = ICodeEditorPartName, Type = typeof(ICodeEditor))]
public sealed partial class CodeEditor : Control, ICodeEditor
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(CodeEditor), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty LineProperty =
        DependencyProperty.Register(nameof(Line), typeof(long), typeof(CodeEditor), new PropertyMetadata(0L));

    public static readonly DependencyProperty ColumnProperty =
        DependencyProperty.Register(nameof(Column), typeof(long), typeof(CodeEditor), new PropertyMetadata(0L));

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(int), typeof(CodeEditor), new PropertyMetadata(100));

    public static readonly DependencyProperty ColorSchemeProperty =
        DependencyProperty.Register(nameof(ColorScheme), typeof(AssemblySyntaxColorScheme), typeof(CodeEditor), new PropertyMetadata(null));

    private const string ICodeEditorPartName = "PART_ICodeEditor";

    private ICodeEditor? _codeEditor;

    public CodeEditor()
    {
        this.DefaultStyleKey = typeof(CodeEditor);
    }

    protected override void OnApplyTemplate()
    {
        _codeEditor = (ICodeEditor)GetTemplateChild(ICodeEditorPartName);
    }

    /// <inheritdoc/>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <inheritdoc/>
    public long Line
    {
        get => (long)GetValue(LineProperty);
        set => SetValue(LineProperty, value);
    }

    /// <inheritdoc/>
    public long Column
    {
        get => (long)GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    /// <inheritdoc/>
    public int Zoom
    {
        get => (int)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    /// <inheritdoc/>
    public AssemblySyntaxColorScheme? ColorScheme
    {
        get => (AssemblySyntaxColorScheme?)GetValue(ColorSchemeProperty);
        set => SetValue(ColorSchemeProperty, value);
    }

    public void NavigateToToken(SourceLocation location) => _codeEditor?.NavigateToToken(location);

    public void ResetHistory() => _codeEditor?.ResetHistory();
}
