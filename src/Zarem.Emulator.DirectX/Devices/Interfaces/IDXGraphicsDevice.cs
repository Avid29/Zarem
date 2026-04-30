// Avishai Dernis 2026

using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Zarem.Emulator.Devices.Interfaces;

/// <summary>
/// An interface for a DirectX powered graphics device.
/// </summary>
public unsafe interface IDXGraphicsDevice : IDevice
{
    /// <summary>
    /// Initialize the graphics.
    /// </summary>
    void InitializeGraphics(ID3D11Device* device, IDXGISwapChain* swapChain);
}
