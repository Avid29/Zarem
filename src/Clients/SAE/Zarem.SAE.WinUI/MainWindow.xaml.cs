// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using WinRT;
using Zarem.Emulator;
using Zarem.SAE.ViewModels;

namespace Zarem.SAE.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ViewModel = new MainViewModel();
    }

    private MainViewModel ViewModel { get; }
}
