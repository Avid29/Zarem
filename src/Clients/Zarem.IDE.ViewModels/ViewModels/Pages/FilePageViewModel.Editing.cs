// Avishai Dernis 2025

using System.IO;
using System.Threading.Tasks;

namespace Zarem.ViewModels.Pages;

public partial class FilePageViewModel
{
    /// <summary>
    /// Gets or sets the content of file.
    /// </summary>
    public string? Content
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(IsDirty));
            }
        }
    }

    private string? OriginalContent
    {
        get;
        set
        {
            Content = value;
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(IsDirty));
            }
        }
    }

    private async Task LoadContentAsync()
    {
        if (File is null)
            return;

        await using var stream = await File.FileItem.OpenStreamForReadAsync();
        using var reader = new StreamReader(stream);
        OriginalContent = await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Saves changes to the file.
    /// </summary>
    public override async Task SaveAsync()
    {
        // TODO: Save as dialog for anonymous files.
        if (File is null)
            return;

        try
        {
            await using var stream = await File.FileItem.OpenStreamForWriteAsync();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(Content ?? string.Empty);
            stream.SetLength(stream.Position);
            OriginalContent = Content;
        }
        catch
        {
            // Ignore errors for now
            return;
        }
    }
}
