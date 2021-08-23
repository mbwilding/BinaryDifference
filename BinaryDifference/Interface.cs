using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace BinaryDifference
{
    public partial class MainWindow
    {
        private void ScrollViewer_PreviewMouseWheel(object s, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)s;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - (double)e.Delta / 5);
            e.Handled = true;
        }

        private void FileBrowse(TextBox fileBox)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                fileBox.Text = fileDialog.SafeFileName;
                fileBox.Uid = fileDialog.FileName;
                StatusBox.Text = fileBox.Uid + " loaded.";
                SaveButton.IsEnabled = false;

                ListBox1.Items.Clear();
                ListBox2.Items.Clear();
            }

            if (File1Box.Uid != string.Empty && File2Box.Uid != string.Empty)
            {
                FileValidation();
            }
        }
        private void FileValidation()
        {
            SaveButton.IsEnabled = false;

            var file1 = new FileInfo(File1Box.Uid);
            var file2 = new FileInfo(File2Box.Uid);

            if (file1.Length == file2.Length)
            {
                CheckDifference(File1Box.Uid, File2Box.Uid);
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
                ListCreate(list, File1Box, ListBox1);
                ListCreate(list, File2Box, ListBox2);
                WriteFile(list, fileDialog.FileName);
            }
        }
        
        private static void ListCreate(ICollection<string> list, UIElement fileBox, ItemsControl listBox)
        {
            list.Add(fileBox.Uid);
            list.Add("-------------");
            list.Add(string.Empty);
            foreach (string item in listBox.Items)
            {
                list.Add(item);
            }

            list.Add(string.Empty);
        }

        private void WriteFile(IEnumerable<string> list, string path)
        {
            using (TextWriter textWriter = new StreamWriter(path))
            {
                foreach (var itemText in list)
                    textWriter.WriteLine(itemText);
            }

            if (File.Exists(path))
            {
                StatusBox.Text = "Saved file: " + path;
            }
            else
            {
                StatusBox.Text = "Saving failed: Check path write permissions.";
            }
        }

        private static string ByteToHex(byte[] buffer, int offset)
        {
            return BitConverter.ToString(buffer, offset, 1);
        }
    }
}
