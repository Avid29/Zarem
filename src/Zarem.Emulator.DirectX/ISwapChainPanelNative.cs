// Avishai Dernis 2026

using Silk.NET.Core.Native;
using Silk.NET.DXGI;
using System;
using System.Runtime.InteropServices;

namespace Zarem.Emulator;

#pragma warning disable CS1591

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("63aad0b8-7c24-40ff-85a8-640d944cc325")]
public unsafe interface ISwapChainPanelNative
{
    [PreserveSig]
    HResult SetSwapChain(IDXGISwapChain* swapChain);

    [PreserveSig]
    ulong Release();
}
