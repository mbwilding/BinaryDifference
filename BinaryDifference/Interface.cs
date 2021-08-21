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

        private void ButtonToggle()
        {
            File1_Button.IsEnabled = !File1_Button.IsEnabled;
            File2_Button.IsEnabled = !File2_Button.IsEnabled;
        }

        private void FileBrowse(TextBox fileBox)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                fileBox.Text = fileDialog.SafeFileName;
                fileBox.Uid = fileDialog.FileName;
                Status_Box.Text = fileBox.Uid + " loaded.";
                Save_Button.IsEnabled = false;

                Listbox1.Items.Clear();
                Listbox2.Items.Clear();
            }

            if (File1_Box.Uid != string.Empty && File2_Box.Uid != string.Empty)
            {
                FileValidation();
            }
        }
        private void FileValidation()
        {
            Save_Button.IsEnabled = false;

            var file1 = new FileInfo(File1_Box.Uid);
            var file2 = new FileInfo(File2_Box.Uid);

            if (file1.Length == file2.Length)
            {
                CheckDifference(File1_Box.Uid, File2_Box.Uid);
            }
            else
            {
                Status_Box.Text = "Files cannot be different sizes.";
            }
        }

        private static void ItemEdit(ItemsControl listBox, int index, string append)
        {
            string content = (String)listBox.Items.GetItemAt(index);
            listBox.Items.RemoveAt(index);
            listBox.Items.Insert(index, content + append);
        }

        private static string ElapsedTime(Stopwatch stopWatch)
        {
            stopWatch.Stop();
            var timeSpan = stopWatch.Elapsed;
            string elapsedTime = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}:{timeSpan.Milliseconds}";
            return elapsedTime;
        }

        private void Finished(Stopwatch stopWatch)
        {
            if (!Listbox1.Items.IsEmpty)
            {
                ButtonToggle();
                Save_Button.IsEnabled = true;
                Status_Box.Text = "Compare completed. Time elapsed: " + ElapsedTime(stopWatch);
            }
            else
            {
                ButtonToggle();
                Status_Box.Text = "Files are identical. Time elapsed: " + ElapsedTime(stopWatch);
            }
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
                Status_Box.Text = "Saved file: " + path;
            }
            else
            {
                Status_Box.Text = "Saving failed: Check path write permissions.";
            }
        }

        private static string StringPrepare(long fileOffset, int bufferOffset, string value)
        {
            return "0x" + (fileOffset + bufferOffset).ToString("X") + ": " + value;
        }

        private static string ByteToHex(byte[] buffer, int offset)
        {
            return BitConverter.ToString(buffer, offset, 1);
        }
    }
}
