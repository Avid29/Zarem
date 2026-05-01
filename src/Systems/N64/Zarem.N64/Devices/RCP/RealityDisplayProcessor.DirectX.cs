// Avishai Dernis 2026

using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System.Runtime.CompilerServices;
using Zarem.N64.Devices.RCP.Enums;
using Zarem.N64.Devices.RCP.Models;

namespace Zarem.N64.Devices.RCP;

#pragma warning disable CS0649

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for processing the display operations.
/// </summary>
public unsafe partial class RealityDisplayProcessor
{
    private const uint MaxVertices = 4; // Quads only need 4 for a TriangleStrip
    private const uint ScreenWidth = 320;
    private const uint ScreenHeight = 240;

    private ComPtr<ID3D11Device> _device;
    private ComPtr<ID3D11DeviceContext> _context;
    private ComPtr<IDXGISwapChain1> _swapChain;
    private ComPtr<ID3D11RenderTargetView> _rtv;
    private ComPtr<ID3D11Buffer> _vertexBuffer;

    /// <summary>
    /// Attaches the DirectX 11 device and swap chain to the RDP for rendering output.
    /// </summary>
    public void AttachGraphics(ID3D11Device* device, IDXGISwapChain1* swapChain)
    {
        _device = new ComPtr<ID3D11Device>(device);
        _swapChain = new ComPtr<IDXGISwapChain1>(swapChain);

        // Get the immediate context from the device
        _device.GetImmediateContext(_context.GetAddressOf());

        // Create the RenderTargetView so we have something to clear
        using ComPtr<ID3D11Texture2D> backBuffer = default;
        _swapChain.GetBuffer(0, SilkMarshal.GuidPtrOf<ID3D11Texture2D>(), (void**)backBuffer.GetAddressOf());
        _device.CreateRenderTargetView((ID3D11Resource*)backBuffer.Handle, (RenderTargetViewDesc*)null, _rtv.GetAddressOf());
    }

    private void CreateBuffers(ID3D11Device* device)
    {
        BufferDesc desc = new()
        {
            ByteWidth = (uint)(sizeof(RdpVertex) * MaxVertices),
            Usage = Usage.Dynamic,
            BindFlags = (uint)BindFlag.VertexBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = (uint)sizeof(RdpVertex)
        };

        device->CreateBuffer(ref desc, null, _vertexBuffer.GetAddressOf());
    }

    private void ProcessCommandList()
    {
        // Important: The RDP can only run if it's not "Frozen" via the Status register
        while (Current < End)
        {
            // Fetch the first word (the OpCode is in the most significant byte)
            var firstWord = _bus.Read<ulong>(Current);
            var opCode = (RdpOpCode)(firstWord >> 56);

            // Identify length and execute
            ExecuteCommand(opCode, Current, out var commandSize);

            // Advance Current
            Current += commandSize;
        }
    }

    private void ExecuteCommand(RdpOpCode opCode, uint address, out uint size)
    {
        size = 8;

        switch (opCode)
        {
            // 24-byte (3-word) Commands
            case RdpOpCode.TextureRectangle:
            case RdpOpCode.TextureRectangleFlip:
                size = 24;
                DrawTextureRectHLE(address);
                break;

            case RdpOpCode.FullSync:
                _swapChain.Present(1, 0);
                break;

            case >= RdpOpCode.Triangle and <= RdpOpCode.TriangleSTZ:
                size = CalculateTriangleSize(opCode);
                //DrawTriangleHLE(opCode, address, size);
                break;

            default:
                break;
        }
    }

    private void DrawTextureRectHLE(uint address)
    {
        // Read the 3 words from Physical Memory
        // These are usually stored big-endian in N64 RDRAM
        ulong w0 = _bus.Read<ulong>(address);
        ulong w1 = _bus.Read<ulong>(address + 8);
        ulong w2 = _bus.Read<ulong>(address + 16);

        // Extract Rect Coordinates (10.2 fixed point)
        float xl = ((w0 >> 12) & 0xFFF) / 4.0f;
        float yl = ((w0 >> 0) & 0xFFF) / 4.0f;
        float xh = ((w0 >> 44) & 0xFFF) / 4.0f;
        float yh = ((w0 >> 32) & 0xFFF) / 4.0f;

        // Extract Texture Parameters (10.5 fixed point)
        uint tileIndex = (uint)((w1 >> 24) & 0x7);
        float s = ((short)((w1 >> 48) & 0xFFFF)) / 32.0f;
        float t = ((short)((w1 >> 32) & 0xFFFF)) / 32.0f;

        // Extract Scaling (5.10 fixed point)
        float dsdx = ((short)((w2 >> 48) & 0xFFFF)) / 1024.0f;
        float dtdy = ((short)((w2 >> 32) & 0xFFFF)) / 1024.0f;

        // Calculate Texture Width/Height for the UVs
        // On N64, the "width" of the texture is defined by dsdx * width_in_pixels
        float rectWidth = xl - xh;
        float rectHeight = yl - yh;
        float maxS = s + (dsdx * rectWidth);
        float maxT = t + (dtdy * rectHeight);

        RenderQuad(xh, yh, xl, yl, s, t, maxS, maxT, tileIndex);
    }

    private void RenderQuad(float xh, float yh, float xl, float yl, float s, float t, float maxS, float maxT, uint tile)
    {
        // Map N64 Screen Space to DirectX NDC (-1 to 1)
        // Formula: (px / screen_width) * 2 - 1
        float left = (xh / ScreenWidth) * 2.0f - 1.0f;
        float right = (xl / ScreenWidth) * 2.0f - 1.0f;
        float top = 1.0f - (yh / ScreenHeight) * 2.0f; // Flip Y for DX
        float bottom = 1.0f - (yl / ScreenHeight) * 2.0f;

        // Prepare the 4 vertices (Triangle Strip: Top-Left, Top-Right, Bottom-Left, Bottom-Right)
        RdpVertex* vertices = stackalloc RdpVertex[(int)MaxVertices];

        vertices[0] = new RdpVertex { Position = new(left, top, 0.5f, 1.0f), TexCoord = new(s, t) };
        vertices[1] = new RdpVertex { Position = new(right, top, 0.5f, 1.0f), TexCoord = new(maxS, t) };
        vertices[2] = new RdpVertex { Position = new(left, bottom, 0.5f, 1.0f), TexCoord = new(s, maxT) };
        vertices[3] = new RdpVertex { Position = new(right, bottom, 0.5f, 1.0f), TexCoord = new(maxS, maxT) };

        // Update the Dynamic Vertex Buffer
        MappedSubresource mapped;
        _context.Map(_vertexBuffer, 0, Map.WriteDiscard, 0, &mapped);
        Unsafe.CopyBlock(mapped.PData, vertices, (uint)(sizeof(RdpVertex) * 4));
        _context.Unmap(_vertexBuffer, 0);

        // Bind Resources and Draw
        uint stride = (uint)sizeof(RdpVertex);
        uint offset = 0;
        ID3D11Buffer* vBuf = _vertexBuffer;
        _context.IASetVertexBuffers(0, 1, &vBuf, &stride, &offset);
        _context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglestrip);

        // TODO: Bind the specific texture tile

        _context.Draw(4, 0);
    }

    private static uint CalculateTriangleSize(RdpOpCode opCode)
    {
        // Base triangle is 1 word for coefficients + 3 words for edges = 4 words (32 bytes)
        uint words = 4;

        if (opCode.HasFlag(RdpOpCode.Tri_S)) words += 4; // Shading (LRGBA) adds 4 words
        if (opCode.HasFlag(RdpOpCode.Tri_T)) words += 4; // Texture (STW) adds 4 words
        if (opCode.HasFlag(RdpOpCode.Tri_Z)) words += 4; // Z-Buffer (Z) adds 4 words

        return words * 8;
    }
}
