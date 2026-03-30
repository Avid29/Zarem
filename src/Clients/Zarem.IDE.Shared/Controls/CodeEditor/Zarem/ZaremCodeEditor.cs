// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Zarem.IDE.Messages.Editor.Enums;
using Zarem.Models.Breakpoints;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor.Zarem;

[TemplatePart(Name = ContentElementPartName, Type = typeof(ScrollViewer))]
[TemplatePart(Name = HighlightBorderPartName, Type = typeof(Border))]
[TemplatePart(Name = DiagnosticOverlayPartName, Type = typeof(Canvas))]
public partial class ZaremCodeEditor : RichEditBox, ICodeEditor
{
    private const string ContentElementPartName = "ContentElement";
    private const string HighlightBorderPartName = "HighlightBorder";
    private const string DiagnosticOverlayPartName = "DiagnosticOverlay";

    private ScrollViewer? _scrollViewer;
    private Border? _highlightBorder;
    private Canvas? _diagnosticOverlay;

    private double _currentLineLogicalY = 0;

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

        _scrollViewer = GetTemplateChild(ContentElementPartName) as ScrollViewer;
        _highlightBorder = GetTemplateChild(HighlightBorderPartName) as Border;
        _diagnosticOverlay = GetTemplateChild(DiagnosticOverlayPartName) as Canvas;

        if (_scrollViewer is not null)
        {
            _scrollViewer.ViewChanged += _scrollViewer_ViewChanged;
        }

        this.Loaded += ZaremCodeEditor_Loaded;
    }

    private void _scrollViewer_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e) => SyncOverlays();

    private void SyncOverlays()
    {
        if (_scrollViewer == null) return;

        // Get the current scroll offsets
        double offsetX = -_scrollViewer.HorizontalOffset;
        double offsetY = -_scrollViewer.VerticalOffset;

        // Sync the Diagnostic Overlay (handles both X and Y for squiggles)
        if (_diagnosticOverlay != null)
        {
            _diagnosticOverlay.RenderTransform = new TranslateTransform
            {
                X = offsetX,
                Y = offsetY
            };
        }

        // Sync the Line Highlight (Usually only needs Y, as it's stretched horizontally)
        if (_highlightBorder != null && _highlightBorder.RenderTransform is TranslateTransform tt)
        {
            // Note: Your existing highlight logic sets tt.Y based on rect.Top.
            // You should now ensure that rect.Top calculation is relative to the
            // UN-SCROLLED text surface, OR simply add offsetY here.

            // If your rect.Top already includes the scroll offset (PointOptions.Client),
            // you might not need to add it again here. 
            // But if you set tt.Y once during SelectionChanged, you need to update it here:
            UpdateHighlightPosition();
        }
    }

    private void UpdateHighlightPosition()
    {
        if (_highlightBorder == null || _scrollViewer == null) return;

        if (_highlightBorder.RenderTransform is TranslateTransform tt)
        {
            // Apply the relative offset
            tt.Y = _currentLineLogicalY - _scrollViewer.VerticalOffset;
        }
    }
}
