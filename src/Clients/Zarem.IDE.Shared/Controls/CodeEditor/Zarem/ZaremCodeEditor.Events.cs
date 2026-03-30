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
        SelectionChanging += ZaremCodeEditor_SelectionChanging;
        SelectionChanged += ZaremCodeEditor_SelectionChanged;
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
    }

    private void ZaremCodeEditor_SelectionChanging(RichEditBox sender, RichEditBoxSelectionChangingEventArgs args)
    {
        var start = args.SelectionStart;
        var end = start + args.SelectionLength;
        SelectedRange = new Range(start, end);
    }

    private void ZaremCodeEditor_SelectionChanged(object sender, RoutedEventArgs e)
    {
        Document.Selection.GetRect(PointOptions.Transform, out Rect rect, out _);

        if (SelectedRange.End.Value - SelectedRange.Start.Value == 0)
        {
            // Highlight the line
            if (_selectedLineHighlightBorder is not null && _selectedLineHighlightBorder?.RenderTransform is TranslateTransform tt)
            {
                _selectedLineHighlightBorder.Visibility = Visibility.Visible;
                tt.Y = rect.Top + Padding.Top;
                _selectedLineHighlightBorder.Height = rect.Height + 2; // TODO: Remove 2 as a magic number
            }
        }
        else if (_selectedLineHighlightBorder is not null)
        {
            // Hide the line highlight
            _selectedLineHighlightBorder.Visibility = Visibility.Collapsed;
        }
    }
}
