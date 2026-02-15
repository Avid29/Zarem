// Avishai Dernis 2025

using System.Threading.Tasks;
using Zarem.DebugSessions;
using Zarem.Models.Files;

namespace Zarem;

public partial class Project
{
    /// <inheritdoc/>
    public DebugSession? StartDebug()
    {
        var emulator = Emulate.CreateEmulator();
        if (emulator is null)
            return null;

        return new DebugSession(emulator);
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

        emulator.Load(module);
        return new DebugSession(emulator);
    }
}
