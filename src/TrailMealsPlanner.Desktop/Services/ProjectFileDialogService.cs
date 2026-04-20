using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace TrailMealsPlanner.Desktop.Services;

public sealed class ProjectFileDialogService
{
    private static readonly FilePickerFileType ProjectFileType = new("TrailMealsPlanner Project")
    {
        Patterns = ["*.trailration"],
        MimeTypes = ["application/json"]
    };

    public async Task<string?> OpenProjectFileAsync()
    {
        var window = GetMainWindow();
        if (window?.StorageProvider is null)
        {
            return null;
        }

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Open ration project",
            FileTypeFilter = [ProjectFileType]
        });

        return files.Count == 0 ? null : files[0].TryGetLocalPath();
    }

    public async Task<string?> SaveProjectFileAsync(string suggestedFileName)
    {
        var window = GetMainWindow();
        if (window?.StorageProvider is null)
        {
            return null;
        }

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save ration project",
            DefaultExtension = "trailration",
            SuggestedFileName = $"{SanitizeFileName(suggestedFileName)}.trailration",
            FileTypeChoices = [ProjectFileType]
        });

        return file?.TryGetLocalPath();
    }

    private static Window? GetMainWindow()
    {
        return (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
    }

    private static string SanitizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "ration-project";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(safeName) ? "ration-project" : safeName;
    }
}
