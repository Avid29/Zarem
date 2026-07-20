// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Zarem.Assembler;
using Zarem.Assembler.Models.Meta;
using Zarem.Assembler.Models.Tables;
using Zarem.IDE.Controls.CheatSheet.Palettes;
using Zarem.IDE.Services;
using Zarem.Mips.Assembler.Models.Meta;
using Zarem.Mips.Models.Instructions.Enums;

namespace Zarem.IDE.Controls.CheatSheet;

/// <summary>
/// A control for displaying usage patterns with colored .
/// </summary>
public sealed partial class UsagePatternDisplay : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsagePatternDisplay"/> class.
    /// </summary>
    public UsagePatternDisplay()
    {
        InitializeComponent();
    }

    public IInstructionMeta? Metadata
    {
        get;
        set
        {
            field = value;
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Gets or sets the brush palette used for displaying arguments in the usage pattern display.
    /// </summary>
    public InstructionTypeBrushPalette? InstructionBrushPalette { get; set; }

    /// <summary>
    /// Gets or sets the brush palette used for displaying arguments in the usage pattern display.
    /// </summary>
    public ArgumentBrushPalette? ArgumentBrushPalette { get; set; }

    private void UpdateDisplay()
    {
        if (Metadata is null)
            return;

        if (Metadata is MipsInstructionMetaBase mipsMeta)
        {
            UpdateNameDisplay(mipsMeta);
            UpdateUsageDisplay(mipsMeta.ArgumentPattern);
        }
    }

    private void UpdateNameDisplay(MipsInstructionMetaBase data)
    {
        // Construct a new Paragraph with the instruction name
        var block = new Paragraph();

        bool isFormatted = false;
        var name = data.Name;
        if (name.EndsWith(".fmt"))
        {
            name = name[..^4]; // Remove the ".fmt" suffix
            isFormatted = true;
        }

        block.Inlines.Add(new Run
        {
            Text = name,
            Foreground = data.Type switch
            {
                MipsInstructionType.BasicR => InstructionBrushPalette?.RType,
                MipsInstructionType.BasicI or MipsInstructionType.IBranch => InstructionBrushPalette?.IType,
                MipsInstructionType.BasicJ => InstructionBrushPalette?.JType,

                MipsInstructionType.RegisterImmediateTrap or
                MipsInstructionType.RegisterImmediateBranch => InstructionBrushPalette?.RegImmediate,

                MipsInstructionType.Special2R or
                MipsInstructionType.Special3R => InstructionBrushPalette?.R2Type,

                MipsInstructionType.Coproc0 => InstructionBrushPalette?.CoProcessor0,

                MipsInstructionType.Coproc1 or
                MipsInstructionType.Float => InstructionBrushPalette?.CoProcessor1,

                // TODO: Pseudo Instructions
                MipsInstructionType.Pseudo => throw new System.NotImplementedException(),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<SolidColorBrush?>(),
            },
        });

        if (isFormatted)
        {
            block.Inlines.Add(new Run
            {
                Text = ".fmt",
                Foreground = ArgumentBrushPalette?.FormatBrush,
            });
        }

        // Clear the existing blocks and add the new one
        NameTextBlock.Blocks.Clear();
        NameTextBlock.Blocks.Add(block);
    }

    private void UpdateUsageDisplay(MipsArgument[] args)
    {
        var usage = new Paragraph();
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            usage.Inlines.Add(CreateArgumentRun(arg));
            if (i != args.Length - 1)
            {
                usage.Inlines.Add(new Run { Text = ", " }); // Add comma between arguments
            }
        }

        UsagePatternTextBlock.Blocks.Clear();
        UsagePatternTextBlock.Blocks.Add(usage);
    }

    private Run CreateArgumentRun(MipsArgument arg)
    {
        var profile = new MipsTokenizerProfile();
        var text = ArgumentTable<MipsArgument>.GetDisplay(arg, profile);

        return arg switch
        {
            MipsArgument.RS or MipsArgument.RT or MipsArgument.RD => new Run
            {
                Text = text,
                Foreground = ArgumentBrushPalette?.GPRegisterBrush,
            },
            MipsArgument.FS or MipsArgument.FT or MipsArgument.FD or MipsArgument.RT_Numbered => new Run
            {
                Text = text,
                Foreground = ArgumentBrushPalette?.CPRegisterBrush,
            },
            MipsArgument.Immediate or MipsArgument.Offset or MipsArgument.Address or
            MipsArgument.ShiftAmount or MipsArgument.FullImmediate => new Run
            {
                Text = text,
                Foreground = ArgumentBrushPalette?.ImmediateValueBrush,
            },
            MipsArgument.AddressBase => new Run
            {
                Text = text,
                Foreground = ArgumentBrushPalette?.MiscArgBrush,
            },
            _ => throw new System.NotImplementedException(),
        };
    }
}
