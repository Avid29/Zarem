// Avishai Dernis 2026

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
}
