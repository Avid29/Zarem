// Avishai Dernis 2026

using Zarem.Emulator.Config;
using Zarem.Emulator.Config.Enums;
using Zarem.Models.Instructions.Enums;

namespace Zarem.N64.Config;

/// <summary>
/// An <see cref="MipsEmulatorConfig"/> for the N64 emulator.
/// </summary>
public class N64EmulatorConfig : MipsEmulatorConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="N64EmulatorConfig"/> class.
    /// </summary>
    public N64EmulatorConfig(ExecutionMode mode) : base(MipsVersion.MipsIII, mode)
    {
    }
}
