using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        private void FileBrowse(TextBox fileBox)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                fileBox.Text = fileDialog.SafeFileName;
                fileBox.Uid = fileDialog.FileName;
                Status_Box.Text = fileBox.Uid + " loaded.";
                Save_Button.IsEnabled = false;

                Dispatcher.Invoke(new ThreadStart(() =>
                    {
                        Listbox1.Items.Clear();
                        Listbox2.Items.Clear();
                    }
                ));
            }

            if (File1_Box.Uid != string.Empty && File2_Box.Uid != string.Empty)
            {
                Compare_Button.IsEnabled = true;
                Save_Button.IsEnabled = false;
            }
        }

        private void FileValidation()
        {
            Save_Button.IsEnabled = false;

            var file1 = new FileInfo(File1_Box.Uid);
            var file2 = new FileInfo(File2_Box.Uid);

            if (file1.Length == file2.Length)
            {
                string path1 = File1_Box.Uid;
                string path2 = File2_Box.Uid;
                Task.Run(() => CheckDifference(path1, path2));
            }
            else
            {
                Status_Box.Text = "Files cannot be different sizes.";
            }
        }

        private static string ElapsedTime(Stopwatch stopWatch)
        {
            stopWatch.Stop();
            var timeSpan = stopWatch.Elapsed;
            string elapsedTime = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}:{timeSpan.Milliseconds}";
            return elapsedTime;
        }

        private void SaveFile()
        {
            var fileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (fileDialog.ShowDialog() == true)
            {
                var list = new List<string>();
                ListCreate(list, File1_Box, Listbox1);
                ListCreate(list, File2_Box, Listbox2);
                WriteFile(list, fileDialog.FileName);
            }
        }

        private static void ListCreate(ICollection<string> list, UIElement fileBox, ItemsControl listBox)
        {
            list.Add(fileBox.Uid);
            list.Add("-------------");
            list.Add(String.Empty);
            foreach (string item in listBox.Items)
            {
                list.Add(item);
            }

            list.Add(String.Empty);
        }

        private void WriteFile(IEnumerable<string> list, string path)
        {
            using (TextWriter textWriter = new StreamWriter(path))
            {
                foreach (string itemText in list)
                    textWriter.WriteLine(itemText);
            }

            if (File.Exists(path))
            {
                Status_Box.Text = "Saved file: " + path;
            }
            else
            {
                Status_Box.Text = "Saving failed: Check path write permissions.";
            }
        }

        private void ButtonToggle()
        {
            File1_Button.IsEnabled = !File1_Button.IsEnabled;
            File2_Button.IsEnabled = !File2_Button.IsEnabled;
            Compare_Button.IsEnabled = !Compare_Button.IsEnabled;
        }
    }
}
