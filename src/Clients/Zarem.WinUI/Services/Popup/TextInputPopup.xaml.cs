// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Zarem.WinUI.Services.Popup;

/// <summary>
/// A <see cref="ContentDialog"/> popup 
/// </summary>
public sealed partial class TextInputPopup : ContentDialog
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextInputPopup), new PropertyMetadata(null));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(TextInputPopup), new PropertyMetadata(null));
    
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(TextInputPopup), new PropertyMetadata(null));
    
    public static readonly DependencyProperty ValidatonRegexProperty =
        DependencyProperty.Register(nameof(ValidatonRegex), typeof(string), typeof(TextInputPopup), new PropertyMetadata(null));

    public TextInputPopup()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the input text.
    /// </summary>
    public string? Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the description text.
    /// </summary>
    public string? Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text in the text box.
    /// </summary>
    public string? PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the validation regex for text confirmation.
    /// </summary>
    public string? ValidatonRegex
    {
        get => (string)GetValue(ValidatonRegexProperty);
        set => SetValue(ValidatonRegexProperty, value);
    }
}
