using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        // Tag used to store the full file path on a Button
        private string _file1Path = string.Empty;
        private string _file2Path = string.Empty;

        private bool _syncingScroll;

        private void File1_Button_Click(object? s, RoutedEventArgs e)
        {
            _ = FileBrowseAsync(isFile1: true);
        }

        private void File2_Button_Click(object? s, RoutedEventArgs e)
        {
            _ = FileBrowseAsync(isFile1: false);
        }

        private void Save_Button_Click(object? s, RoutedEventArgs e)
        {
            _ = SaveFileAsync(FormatComboBox.SelectedIndex == 1);
        }

        private void Scroll1_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (_syncingScroll) return;
            _syncingScroll = true;
            Scroll2.Offset = Scroll1.Offset;
            _syncingScroll = false;
        }

        private void Scroll2_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (_syncingScroll) return;
            _syncingScroll = true;
            Scroll1.Offset = Scroll2.Offset;
            _syncingScroll = false;
        }

        private void FormatComboBox_OnSelectionChanged(object? s, SelectionChangedEventArgs e)
        {
            Format();
            var settings = AppSettings.Load();
            settings.DataFormat = FormatComboBox.SelectedIndex;
            AppSettings.Save(settings);
        }

        private async Task FileBrowseAsync(bool isFile1)
        {
            var topLevel = TopLevel.GetTopLevel(this)!;
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = isFile1 ? "Select File 1" : "Select File 2",
                AllowMultiple = false
            });

            if (files.Count == 0) return;

            var file = files[0];
            var fullPath = file.TryGetLocalPath() ?? file.Name;
            var name = file.Name;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (isFile1)
                {
                    File1Button.Content = name;
                    _file1Path = fullPath;
                }
                else
                {
                    File2Button.Content = name;
                    _file2Path = fullPath;
                }

                StatusBox.Text = fullPath + " loaded.";
                SaveButton.IsEnabled = false;
                Clear();

                if (_file1Path != string.Empty && _file2Path != string.Empty)
                {
                    FileValidation();
                }
            });
        }

        private void FileValidation()
        {
            Differences.Clear();
            SaveButton.IsEnabled = false;

            var file1 = new FileInfo(_file1Path);
            var file2 = new FileInfo(_file2Path);

            if (file1.Length == file2.Length)
            {
                CheckDifference(_file1Path, _file2Path);
            }
            else
            {
                StatusBox.Text = "Files cannot be different sizes.";
            }
        }

        private static string ElapsedTime(Stopwatch stopWatch)
        {
            stopWatch.Stop();
            var timeSpan = stopWatch.Elapsed;
            return $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}:{timeSpan.Milliseconds:000}";
        }

        private async Task SaveFileAsync(bool binaryPatcher)
        {
            var topLevel = TopLevel.GetTopLevel(this)!;

            if (binaryPatcher)
            {
                var listOfItems = ListBox2.Items.Cast<string>().ToList();

                var files = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Binary Patcher File",
                    SuggestedFileName = "patch",
                    FileTypeChoices = new[] { new FilePickerFileType("YAML files") { Patterns = new[] { "*.yml" } } }
                });

                if (files == null) return;

                var savePath = files.TryGetLocalPath()!;

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();

                var patch = new PatchFile
                {
                    Name = "Fill me in",
                    Path = "Fill me in",
                    Payload = new Dictionary<long, string>()
                };

                foreach (var dict in listOfItems.Select(item => item
                             .Replace("{", string.Empty)
                             .Replace("\"", string.Empty)
                             .Replace(" ", string.Empty)
                             .Replace("}", string.Empty)
                             .TrimEnd(',')
                             .Split(',')
                             .ToList()))
                {
                    patch.Payload.Add(Convert.ToInt64(dict[0], 16), dict[1]);
                }

                var patchFile = serializer.Serialize(new List<PatchFile> { patch });
                await File.WriteAllTextAsync(savePath, patchFile);

                StatusBox.Text = File.Exists(savePath) ? "Files Saved." : "Saving failed: Check path write permissions.";
            }
            else
            {
                var files = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Results",
                    SuggestedFileName = "results",
                    FileTypeChoices = new[] { new FilePickerFileType("Text files") { Patterns = new[] { "*.txt" } } }
                });

                if (files == null) return;

                var savePath = files.TryGetLocalPath()!;

                var list1 = new List<string>();
                var list2 = new List<string>();
                ListCreate(list1, _file1Path, ListBox1);
                ListCreate(list2, _file2Path, ListBox2);

                var filePathWithoutExt = Path.ChangeExtension(savePath, null);
                await WriteFileAsync(list1, filePathWithoutExt + "-File1.txt");
                await WriteFileAsync(list2, filePathWithoutExt + "-File2.txt");
            }
        }

        private static void ListCreate(ICollection<string> list, string filePath, ListBox listBox)
        {
            list.Add("File: " + filePath + "\n------------------------------\n");
            foreach (var item in listBox.Items)
            {
                if (item is string s) list.Add(s);
            }
        }

        private async Task WriteFileAsync(IEnumerable<string> list, string path)
        {
            await using var textWriter = new StreamWriter(path);
            foreach (var itemText in list)
                await textWriter.WriteLineAsync(itemText);

            StatusBox.Text = File.Exists(path) ? "Files Saved." : "Saving failed: Check path write permissions.";
        }

        private static string ByteToHex(byte[] buffer, int offset)
        {
            return BitConverter.ToString(buffer, offset, 1);
        }

        public void Clear()
        {
            ListBox1.Items.Clear();
            ListBox2.Items.Clear();
        }

        private class PatchFile
        {
            public string Name { get; init; } = null!;
            public string Path { get; init; } = null!;
            public Dictionary<long, string> Payload { get; init; } = null!;
        }
    }
}
