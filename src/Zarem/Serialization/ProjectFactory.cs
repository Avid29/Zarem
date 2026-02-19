// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using Zarem.Assembler.Config;
using Zarem.Components;
using Zarem.Components.Interfaces;
using Zarem.Config;
using Zarem.Emulator.Config;
using Zarem.Linker.Config;
using Zarem.Registry;

namespace Zarem.Serialization;

/// <summary>
/// A class for creating <see cref="IProject"/> types.
/// </summary>
public static class ProjectFactory
{
    /// <summary>
    /// Constructs an <see cref="IProject"/> from an <see cref="IProjectConfig"/>.
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IProject Create(IProjectConfig config)
    {
        Guard.IsNotNull(config.ArchitectureConfig?.AssemblerConfig);
        Guard.IsNotNull(config.ArchitectureConfig?.LinkerConfig);
        Guard.IsNotNull(config.ArchitectureConfig?.EmulatorConfig);
        Guard.IsNotNull(config.FormatConfig);

        // Retrieve type info
        var archInfo = ZaremRegistry.Architectures.Get(config.ArchitectureConfig.GetType());
        var formatInfo = ZaremRegistry.Formats.Get(config.FormatConfig.GetType());
        Guard.IsNotNull(archInfo);
        Guard.IsNotNull(formatInfo);

        // Create components
        var assemble = CreateHandledComponent<IAssembleComponent, AssemblerConfig>(typeof(AssembleComponent<,>), archInfo.Assembler.AssemblerHandlerType, config.ArchitectureConfig.AssemblerConfig, archInfo.Assembler);
        var linker = CreateHandledComponent<ILinkerComponent, LinkerConfig>(typeof(LinkerComponent<,>), archInfo.Linker.LinkerHandlerType, config.ArchitectureConfig.LinkerConfig, archInfo.Linker);
        var emulate = CreateComponent<IEmulateComponent, EmulatorConfig>(typeof(EmulateComponent<,>), archInfo.Emulator.EmulatorType, config.ArchitectureConfig.EmulatorConfig, archInfo.Emulator);
        var format = CreateComponent<IFormatComponent, FormatConfig>(typeof(FormatComponent<,>), formatInfo.FormatType, config.FormatConfig, formatInfo);

        var project = new Project(config, assemble, emulate, linker, format);
        Guard.IsNotNull(project);
        
        return project;
    }

    /// <summary>
    /// Loads a <see cref="IProject"/> from XML.
    /// </summary>
    /// <param name="path">The path to the config file.</param>
    /// <returns>The loaded <see cref="IProject"/>.</returns>
    public static IProject Load(string path)
    {
        var config = ProjectSerializer.Deserialize(path);
        return Create(config);
    }

    private static T CreateHandledComponent<T, TConfig>(Type openType, Type primaryType, TConfig config, object descripter)
        where T : IProjectComponent
        where TConfig : notnull
    {
        // Form a closed-type format component
        var closedType = openType.MakeGenericType(primaryType, config.GetType());

        // Instantiate
        var handler = Activator.CreateInstance(primaryType, config);
        var component = (T?)Activator.CreateInstance(closedType, handler, config, descripter);
        Guard.IsNotNull(component);

        return component;
    }

    private static T CreateComponent<T, TConfig>(Type openType, Type primaryType, TConfig config, object descripter)
        where T : IProjectComponent
        where TConfig : notnull
    {
        // Form a closed-type format component
        var closedType = openType.MakeGenericType(primaryType, config.GetType());

        // Instantiate
        var component = (T?)Activator.CreateInstance(closedType, config, descripter);
        Guard.IsNotNull(component);

        return component;
    }
}
