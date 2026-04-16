// Avishai Dernis 2026

using System;
using Zarem.Descriptors;
using Zarem.Descriptors.Base;
using Zarem.Elf.Config;

namespace Zarem.Elf;

/// <summary>
/// An <see cref="IModuleFormatDescriptor"/> for the elf format.
/// </summary>
public class ElfModuleDescriptor : LocalizedDescriptor<ElfModuleDescriptor>, IModuleFormatDescriptor
{
    /// <inheritdoc/>
    public override string Identifier => "ELF";

    /// <inheritdoc/>
    protected override string ResourceNamespace => "Zarem.Elf.Resources";

    /// <inheritdoc/>
    public string? DisplayName => Localizer["ModuleFormatName"];

    /// <inheritdoc/>
    public override Type ConfigType => typeof(ElfConfig);

    /// <inheritdoc/>
    public Type FormatType => typeof(ElfModule);
}
