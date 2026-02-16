// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;

namespace Zarem.MIPS;

/// <summary>
/// An <see cref="IArchitectureDescriptor"/> for the MIPS architecture.
/// </summary>
[ZaremPlugin]
public class MIPSArchitectureDescriptor : LocalizedDescriptor<MIPSArchitectureDescriptor>, IArchitectureDescriptor
{
    /// <inheritdoc/>
    public override string Identifier => "MIPS";

    /// <inheritdoc/>
    protected override string ResourceNamespace => "Zarem.MIPS.Resources";

    /// <inheritdoc/>
    public string? DisplayName => Localizer["ArchitectureShortName"];

    /// <inheritdoc/>
    public override Type ConfigType => typeof(MIPSArchitectureConfig);

    /// <inheritdoc/>
    public IAssemblerDescriptor Assembler => new MIPSAssemblerDescriptor();

    /// <inheritdoc/>
    public IEmulatorDescriptor Emulator => new MIPSEmulatorDescriptor();
}
