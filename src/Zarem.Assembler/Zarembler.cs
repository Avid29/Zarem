// Adam Dernis 2024

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zarem.Assembler.Config;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Models;
using Zarem.Assembler.Tokenization;
using Zarem.Localization;
using Zarem.Models;
using Zarem.Models.Tables;


namespace Zarem.Assembler;

//                                          Overview
// ------------------------------------------------------------------------------------------------
//     This assembler works in two passes.
//
//     Pass 1 - Alignment Pass:
//      - Track all labels and macros
//      - Assess instruction size
//      - Allocate memory
//        - Note: Memory will be assigned as well where possible,
//          but all memory will be overwritten on the second pass.
//
//     Pass 2 - Realization Pass:
//      - Assemble instructions
//      - Initialize allocated memory
//

/// <summary>
/// The Zarem assembler.
/// </summary>
/// <remarks>
/// Not in love with this name, but I want it in the assembler namespace and
/// I don't want a class that shares a name with its parent's namespace.
/// </remarks>
public partial class Zarembler
{
    private readonly Logger _logger;
    private readonly Module _module;
    private readonly IAssemblerHandler _archHandler;
    private Section _activeSection;

    /// <summary>
    /// Initializes a new instance of the <see cref="Zarembler"/> class.
    /// </summary>
    private Zarembler(IAssemblerHandler archHandler, AssemblerConfig config, Logger? logger = null)
    {
        _logger = logger ?? new Logger();
        _logger.Register(new Localizer("Zarem.Assembler.Resources.Logger", typeof(Zarembler).Assembly));
        Config = config;

        _archHandler = archHandler;
        _module = new Module(_archHandler.GetArchitectureName());
        _activeSection = _module.GetOrCreateSection(".text");
    }

    /// <inheritdoc/>
    public AssemblerConfig Config { get; }

    /// <summary>
    /// Gets the assembler's current address.
    /// </summary>
    public Address CurrentAddress => _activeSection.CurrentAddress;

    /// <summary>
    /// Gets the assembler's logs.
    /// </summary>
    public IReadOnlyList<LogEntry> Logs => [.._logger.CurrentLog.OfType<LogEntry>()];

    /// <summary>
    /// Gets the symbols found by the assembler.
    /// </summary>
    public IReadOnlyList<Symbol> Symbols => [.._module.Symbols.Values];

    /// <summary>
    /// Gets whether or not the assembler failed to assemble a valid module.
    /// </summary>
    public bool Failed => _logger.CurrentFailed;

    /// <summary>
    /// Assembles a string.
    /// </summary>
    public static async Task<AssemblerResult> AssembleAsync(string str, string? filename, IAssemblerHandler archHandler, AssemblerConfig config, Logger? logger = null)
    {
        using var reader = new StringReader(str);
        var assembler = await AssembleAsync(reader, filename, archHandler, config, logger);
        return new AssemblerResult(assembler.Failed, assembler.Logs, assembler.Symbols, assembler._module);
    }

    /// <summary>
    /// Assembles a stream.
    /// </summary>
    public static async Task<AssemblerResult> AssembleAsync(Stream stream, string? filename, IAssemblerHandler archHandler, AssemblerConfig config, Logger? logger = null)
    {
        using var reader = new StreamReader(stream);
        var assembler = await AssembleAsync(reader, filename, archHandler, config, logger);
        return new AssemblerResult(assembler.Failed, assembler.Logs, assembler.Symbols, assembler._module);
    }

    /// <summary>
    /// Assembles an object module from a stream of assembly.
    /// </summary>
    private static async Task<Zarembler> AssembleAsync(TextReader reader, string? filename, IAssemblerHandler archHandler, AssemblerConfig config, Logger? logger = null)
    {
        logger?.Flush();

        var assembler = new Zarembler(archHandler, config, logger);
        var tokens = await Tokenizer.TokenizeAsync(reader, filename);

        // Run the alignment pass on each line
        for (int i = 1; i <= tokens.LineCount; i++)
            assembler.AlignmentPass(tokens[i]);

        // Reset all streams to start
        assembler._activeSection = assembler._module.Sections[".text"];
        foreach (var section in assembler._module.Sections.Values)
            section.Position = 0;

        // Run the realization pass on each line
        for (int i = 1; i <= tokens.LineCount; i++)
            assembler.RealizationPass(tokens[i]);

        return assembler;
    }
}
