// Avishai Dernis 2026

using System;
using System.Text;
using Zarem.Emulator.Executor.Enum;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// An interpreter mimicking the MARS syscall pattern.
/// </summary>
public class ZaremTrapHandler : MipsTrapHandler
{
    /// <inheritdoc/>
    protected override void HandleSyscall(ulong code, MipsTrapContext context)
    {
        switch (code)
        {
            // Print integer
            case 1:
                Console.Write($"{context.A0}");
                break;

            // Print float
            case 2:
                Console.WriteLine($"{context.Computer.Processor.FloatProcessor.Singles[FloatRegister.F12]}");
                break;

            // Print double
            case 3:
                Console.WriteLine($"{context.Computer.Processor.FloatProcessor.Doubles[FloatRegister.F12]}");
                break;

            // Print ascii string
            case 4:
                Console.Write(context.Computer.Memory.ReadString(context.A0, Encoding.ASCII));
                break;

            // Read integer
            case 5:
                context.V0 = (uint)int.Parse(Console.ReadLine() ?? "");
                break;

            // Read float
            case 6:
                context.Computer.Processor.FloatProcessor.Singles[FloatRegister.F0] = float.Parse(Console.ReadLine() ?? "");
                break;

            // Read double
            case 7:
                context.Computer.Processor.FloatProcessor.Doubles[FloatRegister.F0] = double.Parse(Console.ReadLine() ?? "");
                break;

            // Read ascii string
            case 8:
                context.Computer.Memory.Write(context.A0, ReadString(Encoding.ASCII, context.A1));
                break;

            // Stop execution
            case 10:
                context.Computer.RequestShutdown();
                break;

            // Print unicode string
            case 80:
                Console.Write(context.Computer.Memory.ReadString(context.A0, Encoding.BigEndianUnicode));
                break;

            // Read unicode string
            case 81:
                context.Computer.Memory.Write(context.A0, ReadString(Encoding.BigEndianUnicode, context.A1));
                break;

            default:
                throw new NotImplementedException();
        }

        // Increment the PC
        context.Computer.Processor.ProgramCounter += 4;
    }

    private static byte[] ReadString(Encoding encoding, uint maxBytes)
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
