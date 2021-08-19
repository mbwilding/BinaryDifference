using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        // ReSharper disable once StringLiteralTypo
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        // ReSharper disable once IdentifierTypo
        private static extern int memcmp(byte[] buffer1, byte[] buffer2, int count);

        private void ButtonToggle()
        {
            File1_Button.IsEnabled = !File1_Button.IsEnabled;
            File2_Button.IsEnabled = !File2_Button.IsEnabled;
            Compare_Button.IsEnabled = !Compare_Button.IsEnabled;
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
                CheckDifference(File1_Box.Uid, File2_Box.Uid);
            }
            else
            {
                Status_Box.Text = "Files cannot be different sizes.";
            }
        }

        private void CheckDifference(string filePath1, string filePath2)
        {
            new Thread(() =>
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                Dispatcher.Invoke(new ThreadStart(() =>
                    {
                        ButtonToggle();
                        Listbox1.Items.Clear();
                        Listbox2.Items.Clear();
                        Status_Box.Text = "Processing...";
                    }
                ));

                var file1Details = new FileInfo(filePath1);
                int bufferLength = SetBufferSize(file1Details.Length);
                var fileStream1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read);
                var fileStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read);

                long fileOffset = 0;
                while (fileOffset < file1Details.Length)
                {
                    byte[] buffer1 = FileReadBuffer(fileOffset, bufferLength, fileStream1);
                    byte[] buffer2 = FileReadBuffer(fileOffset, bufferLength, fileStream2);
                    bufferLength = buffer1.Length;

                    if (bufferLength != 0)
                    {
                        if (memcmp(buffer1, buffer2, bufferLength) == 0)
                        {
                            fileOffset += bufferLength;
                        }
                        else
                        {
                            int index = 0;
                            int countPrev = -1;
                            for (int bufferOffset = 0; bufferOffset < buffer1.Length; bufferOffset++)
                            {
                                if (buffer1[bufferOffset] != buffer2[bufferOffset])
                                {
                                    string value1 = BitConverter.ToString(buffer1, bufferOffset, 1);
                                    string value2 = BitConverter.ToString(buffer2, bufferOffset, 1);
                                    string box1 = StringPrepare(fileOffset, bufferOffset, value1);
                                    string box2 = StringPrepare(fileOffset, bufferOffset, value1);

                                    if (bufferOffset != countPrev + 1 || bufferOffset == 0)
                                    {
                                        Dispatcher.Invoke(new ThreadStart(() =>
                                            {
                                                index = Listbox1.Items.Add(box1);
                                                Listbox2.Items.Add(box2);
                                            }
                                        ));
                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(new ThreadStart(() =>
                                            {
                                                ItemEdit(Listbox1, index, value1);
                                                ItemEdit(Listbox2, index, value2);
                                            }
                                        ));
                                    }
                                    countPrev = bufferOffset;
                                }
                            }
                            fileOffset += bufferLength;
                        }
                    }
                }

                fileStream1.Dispose();
                fileStream2.Dispose();

                if (Listbox1.Items.IsEmpty)
                {
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                            ButtonToggle();
                            Status_Box.Text = "Files are identical. Time elapsed: " + ElapsedTime(stopWatch);
                        }
                    ));
                }
                else
                {
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                            ButtonToggle();
                            Save_Button.IsEnabled = true;
                            Status_Box.Text = "Compare completed. Time elapsed: " + ElapsedTime(stopWatch);
                        }
                    ));
                }
            }).Start();
        }

        private static byte[] FileReadBuffer(long offset, int bufferSize, FileStream fileStream)
        {
            if (fileStream.Length - offset < bufferSize)
            {
                bufferSize = (int)(fileStream.Length - offset);

                if (bufferSize <= 0)
                {
                    return null;
                }
            }

            byte[] buffer = new byte[bufferSize];
            _ = fileStream.Seek(offset, SeekOrigin.Begin);
            _ = fileStream.Read(buffer, 0, bufferSize);
            return buffer;
        }

        public static int SetBufferSize(long fileSize)
        {
            const int bufferThreshold = 5 * 1024 * 1024;
            const int bufferMax = 2000 * 1024 * 1024;
            const int bufferDivisions = 60;

            if (fileSize / bufferDivisions > bufferMax)
            {
                return bufferMax;
            }
            if (fileSize > bufferThreshold * bufferDivisions)
            {
                return (int)fileSize / bufferDivisions;
            }
            return (int)fileSize;
        }

        private static void ItemEdit(ItemsControl listBox, int index, string append)
        {
            string content = (String)listBox.Items.GetItemAt(index);
            listBox.Items.RemoveAt(index);
            listBox.Items.Insert(index, content + append);
        }

        private string StringPrepare(long fileOffset, int bufferOffset, string value)
        {
            return "0x" + (fileOffset + bufferOffset).ToString("X") + ": " + value;
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
    }
}