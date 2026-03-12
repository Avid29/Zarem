// Avishai Dernis 2025

using System.Threading.Tasks;
using Zarem.Debugger;
using Zarem.DebugSessions;
using Zarem.Models;
using Zarem.Models.Files;

namespace Zarem;

public partial class Project
{
    /// <inheritdoc/>
    public DebugSession? StartDebug(Module module, bool attach = true)
    {
        var emulator = Emulate.CreateEmulator();
        if (emulator is null)
            return null;

        Zebugger? debugger = attach ? Debug.AttachDebugger(emulator.Computer) : null;
        return new DebugSession(this, module, emulator, debugger);
    }

    /// <inheritdoc/>
    public async Task<DebugSession?> StartDebugAsync(ObjectFile file)
    {
        var module = await Format.ImportAsync(file);
        if (module is null)
            return null;

        var emulator = Emulate.CreateEmulator();
        if (emulator is null)
            return null;

        return new DebugSession(this, module, emulator);
    }
}
