// Avishai Dernis 2025

using LibObjectFile.Elf;
using Zarem.Models.Tables.Enums;

namespace Zarem.Elf.Extensions;

/// <summary>
/// A static class containing enum extensions
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Converts a <see cref="SymbolBinding"/> to an <see cref="ElfSymbolBind"/>.
    /// </summary>
    public static ElfSymbolBind ToElf(this SymbolBinding binding)
    {
        return binding switch
        {
            SymbolBinding.Global => ElfSymbolBind.Global,
            SymbolBinding.Weak => ElfSymbolBind.Weak,
            SymbolBinding.Local or _ => ElfSymbolBind.Local,
        };
    }

    /// <summary>
    /// Converts a <see cref="ElfSymbolBind"/> to an <see cref="SymbolBinding"/>.
    /// </summary>
    public static SymbolBinding FromElf(this ElfSymbolBind bind)
    {
        return bind switch
        {
            ElfSymbolBind.Global => SymbolBinding.Global,
            ElfSymbolBind.Weak => SymbolBinding.Weak,
            ElfSymbolBind.Local or _ => SymbolBinding.Local,
        };
    }
}
