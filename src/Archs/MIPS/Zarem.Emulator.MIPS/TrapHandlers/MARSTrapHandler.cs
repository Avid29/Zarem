// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using Zarem.Emulator.Executor.Enum;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// An interpreter mimicking the MARS syscall pattern.
/// </summary>
public class MarsTrapHandler : MipsTrapHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarsTrapHandler"/> class.
    /// </summary>
    /// <param name="computer"></param>
    public MarsTrapHandler(MipsComputer computer) : base(computer)
    {
    }

    /// <inheritdoc/>
    protected override void HandleSyscall(uint code)
    {
        switch (code)
        {
            // Print integer
            case 1:
                Console.Write($"{A0}");
                break;

            // Print float
            case 2:
                Console.WriteLine($"{Computer.Processor.FloatProcessor.Singles[FloatRegister.F12]}");
                break;

            // Print double
            case 3:
                Console.WriteLine($"{Computer.Processor.FloatProcessor.Doubles[FloatRegister.F12]}");
                break;

            // Print string
            case 4:
                Console.Write(Computer.Memory.ReadString(A0));
                break;

            // Read integer
            case 5:
                V0 = (uint)int.Parse(Console.ReadLine() ?? "");
                break;

            // Read float
            case 6:
                Computer.Processor.FloatProcessor.Singles[FloatRegister.F0] = float.Parse(Console.ReadLine() ?? "");
                break;

            // Read double
            case 7:
                Computer.Processor.FloatProcessor.Doubles[FloatRegister.F0] = double.Parse(Console.ReadLine() ?? "");
                break;

            // Read string
            case 8:
                var str = Console.ReadLine();
                Guard.IsNotNull(str);

                // TODO: Cap A1?
                int i;
                var bytes = new byte[A1];
                for (i = 0; i < str.Length && i < (bytes.Length - 1); i++)
                    bytes[i] = Convert.ToByte(str[i]);
                
                bytes[i] = 0; // Null terminate

                // Write to memory
                Computer.Memory.Write(A0, bytes);
                break;

            // Stop execution
            case 10:
                Computer.RequestShutdown();
                break;

            default:
                throw new NotImplementedException();
        }

        // Increment the PC
        Computer.Processor.ProgramCounter += 4;
    }

    /// <inheritdoc/>
    protected override void HandleTrap(MipsTrap trap)
    {
        throw new NotImplementedException();
    }
}
