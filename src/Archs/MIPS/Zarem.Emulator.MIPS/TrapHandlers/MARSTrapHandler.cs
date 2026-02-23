// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using Zarem.Emulator.Executor.Enum;
using Zarem.Emulator.Machine;
using Zarem.Emulator.TrapHandlers;

namespace Zarem.Emulator.Interpreter;

/// <summary>
/// An interpreter mimicking the MARS syscall pattern.
/// </summary>
public class MARSTrapHandler : MIPSTrapHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MARSTrapHandler"/> class.
    /// </summary>
    /// <param name="computer"></param>
    public MARSTrapHandler(MIPSComputer computer) : base(computer)
    {
    }

    /// <inheritdoc/>
    protected override void HandleSyscall(uint code)
    {
        switch (code)
        {
            // Print integer
            case 1:
                Console.WriteLine($"{A0}");
                break;

            // Print float
            case 2:
                // TODO: Print float
                break;

            // Print double
            case 3:
                // TODO: Print double
                break;

            // Print string
            case 4:
                Console.Write(Computer.Memory.ReadString(A0));
                break;

            // Read integer
            case 5:
                V0 = (uint)int.Parse(Console.ReadLine() ?? "");
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
                Computer.Memory.Write(A0, bytes, false);
                break;

            // Stop execution
            case 10:
                Computer.Emulator.ShutDown();
                break;
        }

        // Increment the PC
        Computer.Processor.ProgramCounter += 4;
    }

    /// <inheritdoc/>
    protected override void HandleTrap(MIPSTrap trap)
    {
        throw new NotImplementedException();
    }
}
