// Avishai Dernis 2026

using System;
using System.Reflection;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Descriptors.Base;
using Zarem.Mips.CheatSheet;

namespace Zarem.Mips;

/// <summary>
/// An <see cref="IArchitectureDescriptor"/> for the MIPS architecture.
/// </summary>
[ZaremPlugin]
public class MipsArchitectureDescriptor : LocalizedDescriptor<MipsArchitectureDescriptor>, IArchitectureDescriptor
{
    /// <inheritdoc/>
    public override string Identifier => "MIPS";

    /// <inheritdoc/>
    protected override string ResourceNamespace => "Zarem.MIPS.Resources";

    /// <inheritdoc/>
    public string? DisplayName => Localizer["ArchitectureShortName"];

    /// <inheritdoc/>
    public override Type ConfigType => typeof(MipsArchitectureConfig);

    /// <inheritdoc/>
    public IAssemblerDescriptor Assembler => new MipsAssemblerDescriptor();

    /// <inheritdoc/>
    public ILinkerDescriptor Linker => new MipsLinkerDescriptor();

    /// <inheritdoc/>
    public IComputerDescriptor Computer => new MipsComputerDescriptor();

    /// <inheritdoc/>
    public IDebuggerDescriptor Debugger => new MipsDebuggerDescriptor();

    /// <inheritdoc/>
    public Assembly? CheatSheetAssembly => typeof(MipsCheatSheet).Assembly;
}
