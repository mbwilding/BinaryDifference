using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinaryDifference.Models;
using BinaryDifference.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BinaryDifference.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    // ── Services ────────────────────────────────────────────────────────────
    private readonly IFileDialogService _fileDialog;

    public MainWindowViewModel(IFileDialogService fileDialog)
    {
        _fileDialog = fileDialog;
        _formatIndex = AppSettings.Load().DataFormat;
    }

    // ── Observable properties ────────────────────────────────────────────────

    [ObservableProperty]
    private string _file1Label = "File 1";

    [ObservableProperty]
    private string _file2Label = "File 2";

    [ObservableProperty]
    private string _statusText = "Load two files of equal size to compare.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private bool _hasResults;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    private bool _isProcessing;

    [ObservableProperty]
    private int _formatIndex;

    public bool IsBusy => IsProcessing;
    public bool CanSave => HasResults && !IsProcessing;

    // ── List items (bound to both ListBoxes via separate filtered views) ────

    public ObservableCollection<string> LeftItems { get; } = new();
    public ObservableCollection<string> RightItems { get; } = new();

    // ── Internal state ───────────────────────────────────────────────────────

    private string _file1Path = string.Empty;
    private string _file2Path = string.Empty;
    private readonly List<DiffEntry> _differences = new();

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanPickFile))]
    private async Task PickFile1()
    {
        var path = await _fileDialog.OpenFileAsync("Select File 1");
        if (path is null) return;
        _file1Path = path;
        File1Label = Path.GetFileName(path);
        await TryCompareAsync();
    }

    [RelayCommand(CanExecute = nameof(CanPickFile))]
    private async Task PickFile2()
    {
        var path = await _fileDialog.OpenFileAsync("Select File 2");
        if (path is null) return;
        _file2Path = path;
        File2Label = Path.GetFileName(path);
        await TryCompareAsync();
    }

    private bool CanPickFile() => !IsProcessing;

    [RelayCommand]
    private void ChangeFormat()
    {
        var settings = AppSettings.Load();
        settings.DataFormat = FormatIndex;
        AppSettings.Save(settings);
        RebuildDisplay();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        if (FormatIndex == 1)
            await SaveBinaryPatcherAsync();
        else
            await SaveDefaultAsync();
    }

    // ── Comparison logic ─────────────────────────────────────────────────────

    private async Task TryCompareAsync()
    {
        if (string.IsNullOrEmpty(_file1Path) || string.IsNullOrEmpty(_file2Path))
            return;

        var f1 = new FileInfo(_file1Path);
        var f2 = new FileInfo(_file2Path);

        if (f1.Length != f2.Length)
        {
            StatusText = "Files must be the same size.";
            return;
        }

        await RunComparisonAsync(_file1Path, _file2Path);
    }

    private async Task RunComparisonAsync(string path1, string path2)
    {
        IsProcessing = true;
        HasResults = false;
        PickFile1Command.NotifyCanExecuteChanged();
        PickFile2Command.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        _differences.Clear();
        LeftItems.Clear();
        RightItems.Clear();
        StatusText = "Processing...";

        var stopwatch = Stopwatch.StartNew();

        await Task.Run(async () =>
        {
            await using var fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read);

            const int bufferSize = 1 * 1024 * 1024; // 1 MB
            long offset = 0;
            bool disparity = false;

            while (offset < fs1.Length)
            {
                var buf1 = await FileManager.SegmentReadAsync(offset, bufferSize, fs1);
                var buf2 = await FileManager.SegmentReadAsync(offset, bufferSize, fs2);
                int len = buf1.Length;

                if (FileManager.MemCmp(buf1, buf2, len) != 0)
                {
                    for (int i = 0; i < len; i++)
                    {
                        if (buf1[i] == buf2[i]) { disparity = false; continue; }

                        var h1 = BitConverter.ToString(buf1, i, 1);
                        var h2 = BitConverter.ToString(buf2, i, 1);

                        if (!disparity)
                        {
                            _differences.Add(new DiffEntry(offset + i, h1, h2));
                        }
                        else
                        {
                            int idx = _differences.Count - 1;
                            var prev = _differences[idx];
                            _differences[idx] = prev with { Hex1 = prev.Hex1 + h1, Hex2 = prev.Hex2 + h2 };
                        }
                        disparity = true;
                    }
                }
                else
                {
                    disparity = false;
                }

                offset += len;
            }
        });

        stopwatch.Stop();
        var elapsed = FormatElapsed(stopwatch.Elapsed);

        if (_differences.Count > 0)
        {
            RebuildDisplay();
            HasResults = true;
            StatusText = $"Found {_differences.Count} difference{(_differences.Count == 1 ? "" : "s")}. Elapsed: {elapsed}";
        }
        else
        {
            StatusText = $"Files are identical. Elapsed: {elapsed}";
        }

        IsProcessing = false;
        PickFile1Command.NotifyCanExecuteChanged();
        PickFile2Command.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }

    // ── Display formatting ────────────────────────────────────────────────────

    private void RebuildDisplay()
    {
        LeftItems.Clear();
        RightItems.Clear();

        if (_differences.Count == 0) return;

        if (FormatIndex == 1) // Binary Patcher
        {
            for (int i = 0; i < _differences.Count; i++)
            {
                bool last = i == _differences.Count - 1;
                var d = _differences[i];
                LeftItems.Add(FormatDict(d.Offset, d.Hex1, last));
                RightItems.Add(FormatDict(d.Offset, d.Hex2, last));
            }
        }
        else // Default
        {
            foreach (var d in _differences)
            {
                LeftItems.Add($"{ToHex(d.Offset)}: {d.Hex1}");
                RightItems.Add($"{ToHex(d.Offset)}: {d.Hex2}");
            }
        }
    }

    private static string FormatDict(long offset, string hex, bool last)
        => last
            ? $"{{\"{ToHex(offset)}\", \"{hex}\"}}"
            : $"{{\"{ToHex(offset)}\", \"{hex}\"}},";

    private static string ToHex(long n) => "0x" + n.ToString("X");

    private static string FormatElapsed(TimeSpan t)
        => $"{t.Hours:00}:{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds:000}";

    // ── Save logic ────────────────────────────────────────────────────────────

    private async Task SaveBinaryPatcherAsync()
    {
        var path = await _fileDialog.SaveFileAsync("Save Binary Patcher", "patch", "YAML files", "*.yml");
        if (path is null) return;

        var serializer = new YamlDotNet.Serialization.SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention.Instance)
            .Build();

        var payload = new Dictionary<long, string>();
        foreach (var d in _differences)
            payload[d.Offset] = d.Hex2;

        var patch = new PatchFile { Name = "Fill me in", Path = "Fill me in", Payload = payload };
        var yaml = serializer.Serialize(new List<PatchFile> { patch });
        await File.WriteAllTextAsync(path, yaml);

        StatusText = File.Exists(path) ? "Saved." : "Save failed — check write permissions.";
    }

    private async Task SaveDefaultAsync()
    {
        var path = await _fileDialog.SaveFileAsync("Save Results", "results", "Text files", "*.txt");
        if (path is null) return;

        var stem = Path.ChangeExtension(path, null);
        await WriteListAsync(stem + "-File1.txt", _file1Path, LeftItems);
        await WriteListAsync(stem + "-File2.txt", _file2Path, RightItems);

        StatusText = "Saved.";
    }

    private static async Task WriteListAsync(string path, string filePath, IEnumerable<string> items)
    {
        await using var writer = new StreamWriter(path);
        await writer.WriteLineAsync("File: " + filePath);
        await writer.WriteLineAsync("------------------------------");
        foreach (var line in items)
            await writer.WriteLineAsync(line);
    }

    // ── Nested model ──────────────────────────────────────────────────────────

    private class PatchFile
    {
        public string Name { get; init; } = null!;
        public string Path { get; init; } = null!;
        public Dictionary<long, string> Payload { get; init; } = null!;
    }
}