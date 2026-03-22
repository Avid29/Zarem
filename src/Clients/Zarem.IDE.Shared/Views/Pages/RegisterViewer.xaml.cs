// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.Bindables;
using Zarem.IDE.Models.Enums;
using Zarem.IDE.Services;
using Zarem.IDE.ViewModels.Pages;

namespace Zarem.IDE.Views.Pages;

public sealed partial class RegisterViewer : UserControl
{
    public RegisterViewer()
    {
        this.InitializeComponent();

        ViewModel = Service.Get<RegisterViewerViewModel>();
    }

    private RegisterViewerViewModel ViewModel { get; }

    private static string GetFormatedValue(RegisterDisplayMode mode, ulong value)
    {
        return mode switch
        {
            RegisterDisplayMode.Hex => $"0x{value:X8}",
            RegisterDisplayMode.Decimal or _ => $"{value}",
        };
    }

    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not BindableRegister reg)
            return;

        reg.DisplayMode = (RegisterDisplayMode)(((int)reg.DisplayMode + 1) % 2);
    }

    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem mfi)
            return;

        if (mfi.DataContext is not BindableRegister br)
            return;

        if (mfi.Tag is not RegisterDisplayMode dm)
            return;

        br.DisplayMode = dm;
    }
}
