// Avishai Dernis 2025

using System.Text;
using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Models.Instructions.Enums;

namespace Zarem.IDE.Controls.CodeEditor;

public partial class AssemblyEditor
{
    private const int ErrorIndicatorIndex = 8;
    private const int WarningIndicatorIndex = 9;
    private const int MessageIndicatorIndex = 10;

    /// <summary>
    /// Applies formatting based on a log messages.
    /// </summary>
    public void ApplyLogHighlights(IReadOnlyList<AssemblerEntry> logs)
    {

    }

    /// <summary>
    /// Clears formatting based on a log messages.
    /// </summary>
    public void ClearLogHighlights()
    {

    }

    private async Task RunAssemblerAsync()
    {
        // Skip assembling if disabled
        if (!RealTimeAssembly)
            return;

        // Run assembler and show errors
        try
        {
            var config = AssemblerConfig ?? new MIPSAssemblerConfig(MipsVersion.MipsIII);
            var result = await Zarembler.AssembleAsync(Text, null, new MIPSAssmblerHandler(config), config);
            ApplyLogHighlights(result.Logs);
            UpdateSymbols(result.Symbols);
        }
        catch (Exception)
        {
            // TODO: Notify exception occured
        }
    }

    private void SetupIndicators()
    {

    }

    private bool MeetsThreshold(Severity severity)
    {
        // The severity value is the threshold -1
        return (int)severity < (int)AnnotationThreshold;
    }
}
