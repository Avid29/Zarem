// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Descriptors.Base;

namespace Zarem.RiscV;

/// <summary>
/// An <see cref="IArchitectureDescriptor"/> for the MIPS architecture.
/// </summary>
[ZaremPlugin]
public class RiscVArchitectureDescriptor : LocalizedDescriptor<RiscVArchitectureDescriptor>, IArchitectureDescriptor
{
    /// <inheritdoc/>
    public override string Identifier => "RISC-V";

    /// <inheritdoc/>
    protected override string ResourceNamespace => "Zarem.RiscV.Resources";

    /// <inheritdoc/>
    public string? DisplayName => Localizer["ArchitectureShortName"];

    /// <inheritdoc/>
    public override Type ConfigType => typeof(RiscVArchitectureConfig);

    /// <inheritdoc/>
    public IAssemblerDescriptor Assembler => new RiscVAssemblerDescriptor();

    /// <inheritdoc/>
    public ILinkerDescriptor Linker => new RiscVLinkerDescriptor();

    /// <inheritdoc/>
    public IComputerDescriptor Computer => new RiscVComputerDescriptor();

    /// <inheritdoc/>
    public IDebuggerDescriptor Debugger => new RiscVDebuggerDescriptor();
}
