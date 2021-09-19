using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        private void File1_Button_Click(object s, RoutedEventArgs e)
        {
            FileBrowse(File1Button);
        }

        private void File2_Button_Click(object s, RoutedEventArgs e)
        {
            FileBrowse(File2Button);
        }

        private void Save_Button_Click(object s, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void ScrollViewer_PreviewMouseWheel(object s, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)s;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - (double)e.Delta / 5);
            e.Handled = true;
        }

        private void FormatComboBox_OnSelectionChanged(object s, RoutedEventArgs e)
        {
            Format();
            Properties.Settings.Default.DataFormat = FormatComboBox.SelectedIndex;
            Properties.Settings.Default.Save();
            Debug.WriteLine(Properties.Settings.Default.DataFormat);
        }

        private void FileBrowse(Button file)
        {
            Task.Run(() =>
            {
                var fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == true)
                {
                    Dispatcher.BeginInvoke((Action) (() =>
                    {
                        file.Content = fileDialog.SafeFileName;
                        file.Uid = fileDialog.FileName;
                        StatusBox.Text = file.Uid + " loaded.";
                        SaveButton.IsEnabled = false;
                        Clear();
                        if (File1Button.Uid != string.Empty && File2Button.Uid != string.Empty)
                        {
                            FileValidation();
                        }
                    }));
                }
            });
        }

        private void FileValidation()
        {
            Differences.Clear();

            SaveButton.IsEnabled = false;

            var file1 = new FileInfo(File1Button.Uid);
            var file2 = new FileInfo(File2Button.Uid);

            if (file1.Length == file2.Length)
            {
                CheckDifference(File1Button.Uid, File2Button.Uid);
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
            string elapsedTime = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}:{timeSpan.Milliseconds:000}";
            return elapsedTime;
        }

        private void SaveFile()
        {
            Task.Run(() =>
            {
                var fileDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (fileDialog.ShowDialog() == true)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var list1 = new List<string>();
                        var list2 = new List<string>();
                        ListCreate(list1, File1Button, ListBox1);
                        ListCreate(list2, File2Button, ListBox2);

                        var filePathWithoutExt = Path.ChangeExtension(fileDialog.FileName, null);
                        WriteFile(list1, filePathWithoutExt + "-File1.txt");
                        WriteFile(list2, filePathWithoutExt + "-File2.txt");
                    }));
                }
            });
        }
        
        private static void ListCreate(ICollection<string> list, UIElement fileBox, ItemsControl listBox)
        {
            list.Add(
                "File: " +
                fileBox.Uid +
                "\n------------------------------\n"
                );
            foreach (string item in listBox.Items)
            {
                list.Add(item);
            }
        }

        private void WriteFile(IEnumerable<string> list, string path)
        {
            using (TextWriter textWriter = new StreamWriter(path))
            {
                foreach (var itemText in list)
                    textWriter.WriteLine(itemText);
            }

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
    }
}
