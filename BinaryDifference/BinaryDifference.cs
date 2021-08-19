using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
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
                CheckDifference(File1_Box.Uid, File2_Box.Uid);
            }
            else
            {
                Status_Box.Text = "Files cannot be different sizes.";
            }
        }

        private void CheckDifference(string file1Path, string file2Path)
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

                // ReSharper disable once CommentTypo
                const int bufferMax = 5 * 1024 * 1024;  // Set to 5MB, max buffer is 2GB: 0x7FFFFFC7
                
                int index = 0;
                int bufferLength;
                bool seqDiff = false;
                long offsetLarge = 0;

                var file1Details = new FileInfo(file1Path);
                if (file1Details.Length < bufferMax)
                {
                    bufferLength = (int)file1Details.Length;
                }
                else
                {
                    bufferLength = bufferMax;
                }

                while (offsetLarge < file1Details.Length)
                {
                    byte[] buffer1 = FileReadBuffer(file1Path, offsetLarge, bufferLength);
                    byte[] buffer2 = FileReadBuffer(file2Path, offsetLarge, bufferLength);
                    bufferLength = buffer1.Length;

                    if (bufferLength != 0)
                    {
                        int offsetSmall = 0;

                        foreach (byte _ in buffer1)
                        {
                            string value1 = BitConverter.ToString(buffer1, offsetSmall, 1).Replace("-", string.Empty);
                            string value2 = BitConverter.ToString(buffer2, offsetSmall, 1).Replace("-", string.Empty);

                            if (value1 != value2)
                            {
                                if (!seqDiff)
                                {
                                    seqDiff = true;
                                    string box1 = "0x" + (offsetSmall + offsetLarge).ToString("X") + ": " + value1;
                                    string box2 = "0x" + (offsetSmall + offsetLarge).ToString("X") + ": " + value2;
                                    
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
                            }
                            else
                            {
                                seqDiff = false;
                            }

                            offsetSmall++;

                            if (offsetSmall == bufferLength)
                            {
                                offsetLarge += offsetSmall;
                            }
                        }
                    }
                    
                }

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

        private static byte[] FileReadBuffer(string filePath, long offset, int bufferSize)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
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
        }

        private static void ItemEdit(ItemsControl listBox, int index, string append)
        {
            string content = (String)listBox.Items.GetItemAt(index);
            listBox.Items.RemoveAt(index);
            listBox.Items.Insert(index, content + append);
        }

        private void ButtonToggle()
        {
            File1_Button.IsEnabled = !File1_Button.IsEnabled;
            File2_Button.IsEnabled = !File2_Button.IsEnabled;
            Compare_Button.IsEnabled = !Compare_Button.IsEnabled;
        }

        private static string ElapsedTime(Stopwatch stopWatch)
        {
            stopWatch.Stop();
            var timeSpan = stopWatch.Elapsed;
            string elapsedTime = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
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