// Avishai Dernis 2026

using Test.Archs.Emulator;
using Zarem.RiscV.Emulator.Config;

namespace Test.RiscV.Emulator;

public abstract record RiscVEmulatorTestCase : EmulatorTestCase<RiscVEmulatorConfig>
{
    public RiscVEmulatorTestCase(RiscVEmulatorConfig config, string input) : base(config, input)
    {
    }
}
