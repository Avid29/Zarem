// Avishai Dernis 2026

using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;

namespace Zarem.IDE.Controls.CodeEditor.Zarem;

public partial class ZaremCodeEditor
{
    private void ZaremCodeEditor_Loaded(object sender, RoutedEventArgs e)
    {
        // While loaded, detach the loaded event and attach unloaded event
        this.Loaded -= ZaremCodeEditor_Loaded;
        this.Unloaded += ZaremCodeEditor_Unloaded;

        TextChanging += ZaremCodeEditor_TextChanging;
        TextChanged += ZaremCodeEditor_TextChanged;
        SelectionChanged += ZaremCodeEditor_SelectionChanged;

        NormalizeLineSpacing();
    }

    private void ZaremCodeEditor_Unloaded(object sender, RoutedEventArgs e)
    {
        // Restore the loaded event and detach unloaded event
        this.Loaded += ZaremCodeEditor_Loaded;
        this.Unloaded -= ZaremCodeEditor_Unloaded;
    }

    private async void ZaremCodeEditor_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
    {
        if (!args.IsContentChanging)
            return;

        Document.GetText(TextGetOptions.None, out var str);
        Text = str;

        await UpdateSyntaxHighlightingAsync();
    }

    private void ZaremCodeEditor_TextChanged(object sender, RoutedEventArgs e)
    {
        RefreshLineNumbers();
    }

    private void ZaremCodeEditor_SelectionChanged(object sender, RoutedEventArgs e)
    {
        Document.Selection.GetRect(PointOptions.ClientCoordinates, out Rect rect, out _);

        if (Document.Selection.Length == 0)
        {
            // Highlight the line
            if (_highlightBorder is not null && _highlightBorder?.RenderTransform is TranslateTransform tt)
            {
                _highlightBorder.Visibility = Visibility.Visible;
                _currentLineLogicalY = rect.Top + Padding.Top;
                tt.Y = _currentLineLogicalY - _scrollViewer?.VerticalOffset ?? 0;

                _highlightBorder.Height = rect.Height;
            }
        }
        else if (_highlightBorder is not null)
        {
            // Hide the line highlight
            _highlightBorder.Visibility = Visibility.Collapsed;
        }
    }
}
