// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zarem.Assembler;
using Zarem.Config;
using Zarem.Descriptors;
using Zarem.Emulator.Config;
using Zarem.Emulator.TrapHandlers;
using Zarem.IDE.Messages.Navigation;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Files;
using Zarem.IDE.ViewModels.Pages.Abstract;
using Zarem.IDE.ViewModels.Pages.CheatSheet;
using Zarem.Linker.Config;
using Zarem.MIPS;
using Zarem.Models.Instructions.Enums;
using Zarem.Registry;
using Zarem.Serialization;

namespace Zarem.IDE.ViewModels.Pages;

/// <summary>
/// A view model for a page to create a new project.
/// </summary>
public partial class CreateProjectViewModel : PageViewModel
{
    private readonly IMessenger _messenger;
    private readonly ILocalizationService _localizationService;
    private readonly IFileSystemService _fileSystemService;
    private readonly IProjectService _projectService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheatSheetViewModel"/> class.
    /// </summary>
    public CreateProjectViewModel(IMessenger messenger, ILocalizationService localizationService, IFileSystemService fileSystemService, IProjectService projectService)
    {
        _messenger = messenger;
        _localizationService = localizationService;
        _fileSystemService = fileSystemService;
        _projectService = projectService;

        ModuleFormat = AvailableModuleFormats.FirstOrDefault();
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/PageTitles/CreateNewProject"];

    /// <summary>
    /// Gets or sets the name of the project to create.
    /// </summary>
    public string? ProjectName
    {
        get;
        set
        {
            if(SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(CreationPath));
                OnPropertyChanged(nameof(NameConflict));
                OnPropertyChanged(nameof(ReadyToCreate));
            }
        }
    }

    /// <summary>
    /// Gets or sets the path of the folder to create the folder in.
    /// </summary>
    public string? FolderPath
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(CreationPath));
                OnPropertyChanged(nameof(NameConflict));
                OnPropertyChanged(nameof(ReadyToCreate));
            }
        }
    }

    /// <summary>
    /// Gets the path where the project will be created.
    /// </summary>
    public string? CreationPath
    {
        get
        {
            if (string.IsNullOrEmpty(ProjectName) || string.IsNullOrEmpty(FolderPath))
                return null;

            return Path.Combine(FolderPath, ProjectName);
        }
    }

    /// <summary>
    /// Gets whether or not the 
    /// </summary>
    public bool NameConflict => Path.Exists(CreationPath);

    /// <summary>
    /// Gets or sets the mips version for the project.
    /// </summary>
    public MipsVersion MipsVersion
    {
        get => field;
        set => SetProperty(ref field, value);
    } = MipsVersion.Mips32R2;

    /// <summary>
    /// Gets the list of available mips version options.
    /// </summary>
    public IEnumerable<MipsVersion> MipsVersionOptions =
    [
        MipsVersion.MipsI,
        MipsVersion.MipsII,
        MipsVersion.MipsIII,
        MipsVersion.MipsIII_32Bit,
        MipsVersion.MipsIV,
        MipsVersion.MipsIV_32Bit,
        MipsVersion.MipsV,
        MipsVersion.MipsV_32Bit,
        MipsVersion.Mips32R1,
        MipsVersion.Mips64R1,
        MipsVersion.Mips32R2,
        MipsVersion.Mips64R2,
    ];

    /// <summary>
    /// Gets or sets the selected module format for the project.
    /// </summary>
    public IModuleFormatDescriptor? ModuleFormat
    {
        get => field;
        set => SetProperty(ref field, value); 
    }

    /// <summary>
    /// Gets a list of the avilable formats
    /// </summary>
    public IEnumerable<IModuleFormatDescriptor> AvailableModuleFormats => ZaremRegistry.Formats.GetDescriptors();

    /// <summary>
    /// Gets whether or not the project can be created.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ProjectName), nameof(FolderPath), nameof(ModuleFormat))]
    public bool ReadyToCreate => ProjectName is not null && FolderPath is not null && !NameConflict;

    [RelayCommand]
    private async Task CreateProjectAsync()
    {
        // TODO: Notify errors

        if (!ReadyToCreate)
            return;

        // Attempt to create the project root folder
        var rootFolderPath = Path.Combine(FolderPath, ProjectName);
        var rootFolder = await _fileSystemService.CreateFolderAsync(rootFolderPath);
        if (rootFolder is null)
            return;

        // Attempt to create project file
        var projectFilePath = Path.Combine(rootFolderPath, $"{ProjectName}.zrmp");
        var projectFile = await _fileSystemService.CreateFileAsync(projectFilePath);
        if (projectFile is null)
            return;

        // Attempt to create the config
        var formatConfig = (FormatConfig?)Activator.CreateInstance(ModuleFormat.ConfigType);
        if (formatConfig is null)
            return;

        // Create the file config
        var projectConfig = new ProjectConfig
        {
            Name = ProjectName,
            ConfigPath = projectFilePath,
            ArchitectureConfig = new MipsArchitectureConfig()
            {
                MipsVersion = MipsVersion,
                AssemblerConfig = new MipsAssemblerConfig(),
                EmulatorConfig = new MipsEmulatorConfig()
                {
                    TrapHost = new ZaremTrapHandler(),
                },
                LinkerConfig = new MipsLinkerConfig(),
            },
            FormatConfig = formatConfig,
        };

        // Write project config to the file 
        ProjectSerializer.Serialize(projectConfig, projectFilePath);

        // Open the project and close the page
        await _projectService.OpenProjectAsync(projectConfig);
        ClosePage();

        // TODO: Open the project in a new window?
    }

    [RelayCommand]
    private async Task SelectFolderAsync()
    {
        var folder = await _fileSystemService.PickFolderAsync();
        if (folder is null)
            return;

        FolderPath = folder.Path;
    }

    [RelayCommand]
    private void Cancel() => ClosePage();

    private void ClosePage() => _messenger.Send(new PageCloseRequestMessage(this));
}
