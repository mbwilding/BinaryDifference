using System.Threading.Tasks;

namespace BinaryDifference.Services;

/// <summary>
/// Abstracts platform file dialogs so the ViewModel stays testable.
/// </summary>
public interface IFileDialogService
{
    Task<string?> OpenFileAsync(string title);
    Task<string?> SaveFileAsync(string title, string suggestedName, string typeName, string pattern);
}