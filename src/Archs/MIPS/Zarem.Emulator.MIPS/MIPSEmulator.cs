// Avishai Dernis 2025

using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Models.Enums;
using Zarem.Models;

namespace Zarem.Emulator;

/// <summary>
/// An emulator of a MIPS machine.
/// </summary>
public class MIPSEmulator : Emulator<MIPSEmulatorConfig>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MIPSEmulator"/> class.
    /// </summary>
    public MIPSEmulator(MIPSEmulatorConfig config) : base(config)
    {
        Computer = new MIPSComputer(config);
    }

    /// <summary>
    /// Gets the computer system the interpreter is emulating.
    /// </summary>
    public MIPSComputer Computer { get; }

    /// <summary>
    /// Loads a <see cref="Module"/> to the interpreter's memory.
    /// </summary>
    /// <param name="module">The module to load.</param>
    public override void Load(Module module)
    {
        var destination = Computer.Memory.AsStream();

        foreach (var section in module.Sections.Values)
        {
            var vAddr = (long)section.VirtualAddress;
            if (destination.Length < vAddr)
            {
                destination.SetLength(vAddr);
            }

            destination.Position = vAddr;
            section.Stream.Position = 0;
            section.Stream.CopyTo(destination);
        }

        if (module.EntryAddress is not null)
        {
            Computer.Processor.ProgramCounter = (uint)module.EntryAddress;
        }

        State = EmulatorState.Ready;
    }

    /// <inheritdoc/>
    protected override void Tick() => Computer.Tick();
}
