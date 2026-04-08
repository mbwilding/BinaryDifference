using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace BinaryDifference.Services;

public class AvaloniaFileDialogService : IFileDialogService
{
    private readonly TopLevel _topLevel;

    public AvaloniaFileDialogService(TopLevel topLevel)
    {
        _topLevel = topLevel;
    }

    public async Task<string?> OpenFileAsync(string title)
    {
        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task<string?> SaveFileAsync(string title, string suggestedName, string typeName, string pattern)
    {
        var file = await _topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedName,
            FileTypeChoices = new[]
            {
                new FilePickerFileType(typeName) { Patterns = new[] { pattern } }
            }
        });

        return file?.TryGetLocalPath();
    }
}