// Avishai Dernis 2026

using Microsoft.UI.Xaml;

namespace Zarem.IDE.Helpers;

public class ViewModelProxy : DependencyObject
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(object), typeof(ViewModelProxy), new PropertyMetadata(null));

    public object ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
}
