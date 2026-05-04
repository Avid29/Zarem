// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Linker.Logging;

namespace Zarem.Mips.Linker.Logging;

/// <summary>
/// A logger which simplifies logging events in the linker.
/// </summary>
internal class LinkerLogger : LocalLogger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LinkerLogger"/> class.
    /// </summary>
    public LinkerLogger(ILogger parent) : base(parent, "Zarem.Mips.Linker.Resources.Logger", typeof(LinkerLogger).Assembly)
    {
    }

    public bool Log(Severity severity, LogId id, string filePath, string messageKey, params object?[] args)
    {
        var message = Localizer[messageKey, args];
        Guard.IsNotNull(message);

        return Parent.Log(new LinkerLogEntry(severity, new LogCode("MPS_LNK", (uint)id), message, filePath));
    }
}
