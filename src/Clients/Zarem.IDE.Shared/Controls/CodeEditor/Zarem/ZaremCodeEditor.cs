// Avishai Dernis 2026

using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
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
    private const string GutterScrollViewerPartName = "GutterScrollViewer";
    private const string LineNumberContainerPartName = "LineNumberContainer";

    private ScrollViewer? _scrollViewer;
    private Canvas? _diagnosticOverlay;
    private Border? _highlightBorder;
    private ScrollViewer? _gutterScroll;
    private StackPanel? _lineNumberStack;

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
        _gutterScroll = GetTemplateChild(GutterScrollViewerPartName) as ScrollViewer;
        _lineNumberStack = GetTemplateChild(LineNumberContainerPartName) as StackPanel;
        _highlightBorder = GetTemplateChild(HighlightBorderPartName) as Border;
        _diagnosticOverlay = GetTemplateChild(DiagnosticOverlayPartName) as Canvas;

        if (_scrollViewer is not null)
        {
            _scrollViewer.ViewChanged += _scrollViewer_ViewChanged;
        }

        this.Loaded += ZaremCodeEditor_Loaded;
    }

    private void _scrollViewer_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        _gutterScroll?.ChangeView(null, _scrollViewer?.VerticalOffset, null, true);
        SyncOverlays();
    }

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

    private void RefreshLineNumbers()
    {
        if (_lineNumberStack is null)
            return;

        // Get total lines using the Text Object Model
        Document.GetText(TextGetOptions.None, out string fullText);
        var range = Document.GetRange(fullText.Length, fullText.Length);
        var totalLines = range.GetIndex(TextRangeUnit.Line);

        // Get the actual height of a single line of text
        Document.Selection.GetRect(PointOptions.ClientCoordinates, out Rect rect, out _);
        double lineHeight = rect.Height > 0 ? rect.Height : 20; // Fallback

        if (_lineNumberStack.Children.Count != totalLines)
        {
            _lineNumberStack.Children.Clear();
            for (int i = 1; i <= totalLines; i++)
            {
                _lineNumberStack.Children.Add(new TextBlock
                {
                    Text = i.ToString(),
                    Height = lineHeight,
                    FontSize = this.FontSize,
                    Foreground = new SolidColorBrush(Colors.DimGray),
                    TextAlignment = TextAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
        }
    }

    private void NormalizeLineSpacing()
    {
        // 1. Select the entire document
        var allRange = Document.GetRange(0, int.MaxValue);

        // 2. Set a fixed line spacing rule
        // LineSpacingRule 4 = Fixed (Exactly)
        // 20 is the height in points. Adjust this to match your TextBlock height.
        allRange.ParagraphFormat.SetLineSpacing(LineSpacingRule.Exactly, 16);

        // 3. Remove paragraph margins that cause "drift"
        allRange.ParagraphFormat.SpaceBefore = 0;
        allRange.ParagraphFormat.SpaceAfter = 0;
    }
}
