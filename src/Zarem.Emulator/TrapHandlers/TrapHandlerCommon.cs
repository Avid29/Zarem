// Avishai Dernis 2026

using System;
using System.Runtime.CompilerServices;
using System.Text;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.TrapHandlers.Interfaces;
using Zarem.Models.Enums;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// A collection of common methods for trap handlers. This is used to avoid code duplication between different trap handlers.
/// </summary>
public static class TrapHandlerCommon
{
    /// <summary>
    /// Prints the integer in arg0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PrintInteger(ITrapContext context) => Console.Write(context.Argument0);

    /// <summary>
    /// Prints the float in farg0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PrintFloat(ITrapContext context) => Console.Write(context.FloatArgument0);

    /// <summary>
    /// Prints the float in farg0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PrintDouble(ITrapContext context) => Console.Write(context.DoubleArgument0);
    /// <summary>
    /// Prints a null-terminated string at arg0, encoded with the given encoding.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PrintString(ITrapContext context) => PrintString(context, GetEncoding(context));

    /// <summary>
    /// Prints a null-terminated string at arg0, encoded with the given encoding.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PrintString(ITrapContext context, Encoding encoding) => Console.Write(context.Cpu.Memory.ReadString(context.Argument0, encoding));

    /// <summary>
    /// Reads an integer into ret0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadInteger(ITrapContext context) => context.Result0 = (uint)int.Parse(Console.ReadLine() ?? "");

    /// <summary>
    /// Reads a string into the designated buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadString(ITrapContext context) => ReadString(context, GetEncoding(context));

    /// <summary>
    /// Reads a string into the designated buffer.
    /// </summary>
    public static void ReadString(ITrapContext context, Encoding encoding)
    {
        var str = Console.ReadLine() ?? string.Empty;

        // Determine null-terminator size (1 for UTF8, 2 for UTF16, etc.)
        int stride = encoding.GetByteCount("\0");

        // We must leave 'stride' bytes at the end for the \0
        int maxBytes = (int)context.Argument1;
        int maxDataBytes = maxBytes - stride;
        if (maxDataBytes < 0)
            return;

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

        // Write to the buffer
        context.Cpu.Memory.Write(context.Argument0, bytes);
    }

    /// <summary>
    /// Reads a float into fret0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadFloat(ITrapContext context) => context.FloatResult0 = float.Parse(Console.ReadLine() ?? "");

    /// <summary>
    /// Reads a double into fret0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadDouble(ITrapContext context) => context.DoubleResult0 = double.Parse(Console.ReadLine() ?? "");

    private static Encoding GetEncoding(ITrapContext context)
    {
        return context.Argument2 switch
        {
            0 => Encoding.ASCII,
            1 => Encoding.UTF8,
            2 => context.Cpu.Endianness is Endianness.Big ? Encoding.BigEndianUnicode : Encoding.Unicode,
            _ => throw new ArgumentOutOfRangeException(nameof(context), "Invalid encoding type"),
        };
    }
}
