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
public unsafe sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ViewModel = new MainViewModel();
    }

    private MainViewModel ViewModel { get; }

    private void GraphicsOutputPanel_Loaded(object sender, RoutedEventArgs e)
    {
        CreateDx11Resources(GraphicsOutputPanel, out var device, out var context, out var swapChain);

        ViewModel.N64.RealityCoProcessor.InitializeGraphics(device, swapChain);
        GraphicsOutputPanel.As<ISwapChainPanelNative>().SetSwapChain((IDXGISwapChain*)swapChain);
    }

    private static void CreateDx11Resources(SwapChainPanel panel, out ID3D11Device* device, out ID3D11DeviceContext* context, out IDXGISwapChain1* swapChain)
    {
        var d3d11 = D3D11.GetApi(null);

        using ComPtr<ID3D11Device> comDevice = default;
        using ComPtr<ID3D11DeviceContext> comContext = default;

        d3d11.CreateDevice(null, D3DDriverType.Hardware, 0, (uint)CreateDeviceFlag.BgraSupport,
            null, 0, D3D11.SdkVersion, comDevice.GetAddressOf(), null, comContext.GetAddressOf());

        using ComPtr<IDXGIDevice1> dxgiDevice = default;
        comDevice.QueryInterface(SilkMarshal.GuidPtrOf<IDXGIDevice1>(), (void**)dxgiDevice.GetAddressOf());

        using ComPtr<IDXGIAdapter> adapter = default;
        dxgiDevice.GetAdapter(adapter.GetAddressOf());

        using ComPtr<IDXGIFactory2> factory = default;
        adapter.GetParent(SilkMarshal.GuidPtrOf<IDXGIFactory2>(), (void**)factory.GetAddressOf());

        SwapChainDesc1 desc = new()
        {
            Width = (uint)Math.Max(1, panel.ActualWidth),
            Height = (uint)Math.Max(1, panel.ActualHeight),
            Format = Format.FormatB8G8R8A8Unorm,
            Stereo = (Silk.NET.Core.Bool32)false,
            SampleDesc = new SampleDesc(1, 0),
            BufferUsage = DXGI.UsageRenderTargetOutput,
            BufferCount = 2,
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipSequential,
            AlphaMode = AlphaMode.Premultiplied,
            Flags = 0
        };

        using ComPtr<IDXGISwapChain1> comSwapChain = default;
        factory.CreateSwapChainForComposition((IUnknown*)comDevice.Handle, ref desc, null, comSwapChain.GetAddressOf());

        device = comDevice.Detach();
        context = comContext.Detach();
        swapChain = comSwapChain.Detach();
    }
}
