// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using Zarem.Assembler.Logging;
using Zarem.IDE.Messages.Editor.Enums;
using Zarem.Models.Breakpoints;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor.Zarem;

[TemplatePart(Name = SelectedLineHighlightBorderPartName, Type = typeof(Border))]
public partial class ZaremCodeEditor : RichEditBox, ICodeEditor
{
    private const string SelectedLineHighlightBorderPartName = "SelectedLineHighlightBorder";

    private Border? _selectedLineHighlightBorder;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyEditBox"/> class.
    /// </summary>
    public ZaremCodeEditor()
    {
        DefaultStyleKey = typeof(ZaremCodeEditor);
    }

    public void ApplyOperation(EditorOperation operation)
    {
    }

    public void ApplyLogHighlights(IReadOnlyList<AssemblerEntry> logs)
    {
    }

    public void ClearLogHighlights()
    {
    }

    public void NavigateToToken(SourceLocation location)
    {
    }

    public void RegisterBreakpointSource(BreakpointCollection breakpoints)
    {
    }

    public void UnregisterBreakpointSource()
    {
    }

    public void ResetHistory()
    {
        Document.ClearUndoRedoHistory();
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _selectedLineHighlightBorder = GetTemplateChild(SelectedLineHighlightBorderPartName) as Border;

        this.Loaded += ZaremCodeEditor_Loaded;
    }
}
