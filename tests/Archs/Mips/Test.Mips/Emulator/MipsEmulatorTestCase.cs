// Avishai Dernis 2026

using Test.Archs.Emulator;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Machine.Registers.CoProcessor0;

namespace Test.Mips.Emulator;

public abstract record MipsEmulatorTestCase : EmulatorTestCase<MipsEmulatorConfig>
{
    public MipsEmulatorTestCase(MipsEmulatorConfig config, string input) : base(config, input)
    {
    }

    public StatusRegister Status { get; init; }
}
