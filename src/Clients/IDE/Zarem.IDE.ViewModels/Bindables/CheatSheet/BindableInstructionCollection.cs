// Avishai Dernis 2026

using System.Collections.ObjectModel;
using Zarem.Assembler.Models.Meta;
using Zarem.CheatSheet.Models;

namespace Zarem.IDE.Bindables.CheatSheet;

/// <summary>
/// A bindable wrapper for the <see cref="InstructionCollection"/>.
/// </summary>
public class BindableInstructionCollection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BindableInstructionCollection"/> class.
    /// </summary>
    public BindableInstructionCollection(InstructionCollection collection, IInstructionMeta[] metas)
    {
        Name = collection.Name;
        Groups = [];

        foreach (var group in collection.Groups)
        {
            var bGroup = new BindableInstructionGroup(group, metas);
            if (bGroup.Metas.Length is 0)
                continue;

            Groups.Add(bGroup);
        }
    }

    /// <summary>
    /// Gets the name of the collection.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the collection of groups.
    /// </summary>
    public ObservableCollection<BindableInstructionGroup> Groups { get; }
}
