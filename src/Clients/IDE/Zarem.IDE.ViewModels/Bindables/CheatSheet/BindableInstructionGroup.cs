// Avishai Dernis 2026

using System.Linq;
using Zarem.Assembler.Models.Meta;
using Zarem.CheatSheet.Models;

namespace Zarem.IDE.Bindables.CheatSheet;

/// <summary>
/// A bindable wrapper for the <see cref="InstructionGroup"/>.
/// </summary>
public class BindableInstructionGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BindableInstructionGroup"/> class.
    /// </summary>
    public BindableInstructionGroup(InstructionGroup group, IInstructionMeta[] metas)
    {
        Name = group.Name;
        Metas = [.. group.Instructions
            .Where(x => metas.Any(meta => meta.Identifier == x))
            .Select(x => metas.First(meta => meta.Identifier == x))];
    }

    /// <summary>
    /// Gets the name of the group.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the instruction meta data.
    /// </summary>
    public IInstructionMeta[] Metas { get; }
}
