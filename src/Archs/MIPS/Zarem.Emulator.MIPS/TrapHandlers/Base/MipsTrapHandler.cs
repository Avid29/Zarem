// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Zarem.Emulator.Exceptions;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models.Enum;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.TrapHandlers.Base;

/// <summary>
/// An interface for an interpreter, which handles traps as the host-layer
/// </summary>
public abstract class MipsTrapHandler : ITrapHandler
{
    /// <summary>
    /// A method to handle syscalls.
    /// </summary>
    /// <param name="code">The syscall code.</param>
    /// <param name="context">The trap context</param>
    protected abstract void HandleSyscall(ulong code, MipsTrapContext context);

    /// <summary>
    /// A method to direct trap handling.
    /// </summary>
    /// <param name="context">The context of the trap.</param>
    protected virtual void HandleTrap(MipsTrapContext context)
    {
        switch ((MipsTrap)context.TrapCode)
        {
            case MipsTrap.Syscall:
                HandleSyscall(context.V0, context);
                context.Cpu.ProgramCounter += 4;
                break;
            case MipsTrap.ReservedInstruction:
                throw new ReservedInstructionException(context.Cpu.ProgramCounter);
        }
    }

    /// <inheritdoc/>
    public void HandleTrap(ICpu cpu, ulong trapCode)
    {
        if (cpu is not IMipsCpu mipsCpu)
        {
            ThrowHelper.ThrowArgumentException(nameof(mipsCpu));
            return;
        }

        HandleTrap(new MipsTrapContext(mipsCpu, trapCode));
    }

    /// <summary>
    /// Prints the integer in $a0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void PrintInteger(MipsTrapContext context) => Console.Write($"{context.A0}");

    /// <summary>
    /// Prints the float in $f12.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void PrintFloat(MipsTrapContext context) => Console.Write($"{context.Cpu.FloatProcessor.Singles[FloatRegister.F12]}");

    /// <summary>
    /// Prints the double in $f12.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void PrintDouble(MipsTrapContext context) => Console.Write($"{context.Cpu.FloatProcessor.Doubles[FloatRegister.F12]}");

    /// <summary>
    /// Prints the ASCII string found at the address in $a0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void PrintString(MipsTrapContext context, Encoding encoding) => Console.Write(context.Cpu.Memory.ReadString(context.A0, encoding));

    /// <summary>
    /// Reads an integer into $v0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void ReadInteger(MipsTrapContext context) => context.V0 = (uint)int.Parse(Console.ReadLine() ?? "");

    /// <summary>
    /// Reads a float into $f0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void ReadFloat(MipsTrapContext context) => context.Cpu.FloatProcessor.Singles[FloatRegister.F0] = float.Parse(Console.ReadLine() ?? "");

    /// <summary>
    /// Reads a float into $f0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void ReadDouble(MipsTrapContext context) => context.Cpu.FloatProcessor.Doubles[FloatRegister.F0] = double.Parse(Console.ReadLine() ?? "");

    /// <summary>
    /// Reads a float into $f0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void ReadString(MipsTrapContext context, Encoding encoding) => context.Cpu.Memory.Write(context.A0, ReadString(encoding, context.A1));

    /// <summary>
    /// Reads a float into $f0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void Shutdown(MipsTrapContext context) => context.Cpu.RequestShutdown();

    private static byte[] ReadString(Encoding encoding, ulong maxBytes)
    {
        var str = Console.ReadLine() ?? string.Empty;

        // Determine null-terminator size (1 for UTF8, 2 for UTF16, etc.)
        int stride = encoding.GetByteCount("\0");

        // We must leave 'stride' bytes at the end for the \0
        int maxDataBytes = (int)maxBytes - stride;
        if (maxDataBytes < 0)
            return [];

        byte[] bytes = new byte[maxBytes];
        var encoder = encoding.GetEncoder();

        // Apply encoding
        encoder.Convert(
            chars: str.AsSpan(),
            bytes: bytes.AsSpan(0, maxDataBytes),
            flush: true,
            out int _,
            out int bytesUsed,
            out bool _);

        // Apply null terminator and resize the array to only the bytes used.
        Array.Resize(ref bytes, bytesUsed + stride);
        bytes.AsSpan(bytesUsed, stride).Clear();
        return bytes;
    }
}
