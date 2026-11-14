// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Zarem.IDE.Controls;

public sealed partial class EditableTextBlock : Control
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(EditableTextBlock), new(null));

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(EditableTextBlock), new(null));

    public const string TextBlockPartName = "PART_TextBlock";
    public const string TextBoxPartName = "PART_TextBox";

    private TextBlock? _textBlock;
    private TextBox? _textBox;

    public EditableTextBlock()
    {
        DefaultStyleKey = typeof(EditableTextBlock);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _textBlock = (TextBlock)GetTemplateChild(TextBlockPartName);
        _textBox = (TextBox)GetTemplateChild(TextBoxPartName);

        _textBlock.Visibility = Visibility.Visible;
        _textBox.Visibility = Visibility.Collapsed;

        _textBox.LostFocus += OnTextBoxLostFocus;
        Unloaded += OnUnloaded;
    }

    public void BeginEdit()
    {
        if (_textBlock is null || _textBox is null)
            return;

        _textBlock.Visibility = Visibility.Collapsed;
        _textBox.Visibility = Visibility.Visible;

        _textBox.Focus(FocusState.Programmatic);
        _textBox.Text = Text;

        // Select file name without extension
        var selectionEnd = Text.LastIndexOf('.');
        if (selectionEnd is -1)
        {
            selectionEnd = Text.Length;
        }

        _textBox.Select(0, selectionEnd);
    }

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs args)
    {
        if (_textBlock is null || _textBox is null)
            return;

        Text = _textBox.Text;
        _textBlock.Visibility = Visibility.Visible;
        _textBox.Visibility = Visibility.Collapsed;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_textBlock is null || _textBox is null)
            return;

        _textBox.LostFocus -= OnTextBoxLostFocus;
        Unloaded -= OnUnloaded;
    }
}
