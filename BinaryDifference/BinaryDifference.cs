using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]

    public partial class MainWindow
    {
        private void FileBrowse(TextBox box)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                box.Text = fileDialog.SafeFileName;
                box.Uid = fileDialog.FileName;
                CompareStatus_Box.Text = box.Uid + " loaded.";
                Save_Button.IsEnabled = false;

                Dispatcher.Invoke(new ThreadStart(() =>
                    {
                        Compare_Listbox1.Items.Clear();
                        Compare_Listbox2.Items.Clear();
                    }
                ));
            }

            if (CompareFile1_Box.Uid != string.Empty && CompareFile2_Box.Uid != string.Empty)
            {
                Compare_Button.IsEnabled = true;
            }
        }

        private void FileValidation()
        {
            FileInfo file1 = new FileInfo(CompareFile1_Box.Uid);
            FileInfo file2 = new FileInfo(CompareFile2_Box.Uid);

            if (file1.Length == file2.Length)
            {
                CheckDifference(CompareFile1_Box.Uid, CompareFile2_Box.Uid);
            }
            else
            {
                CompareStatus_Box.Text = "Files cannot be different sizes.";
            }
        }

        private static byte[] FileReadBuffer(string filePath, long offset, int bufferSize)
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            {
                if (fs.Length - offset < bufferSize)
                {
                    bufferSize = (int)(fs.Length - offset);

                    if (bufferSize <= 0)
                    {
                        return null;
                    }
                }

                byte[] buffer = new byte[bufferSize];
                _ = fs.Seek(offset, SeekOrigin.Begin);
                _ = fs.Read(buffer, 0, bufferSize);
                return buffer;
            }
        }

        private void CheckDifference(string file1Path, string file2Path)
        {
            new Thread(() =>
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                Dispatcher.Invoke(new ThreadStart(() =>
                    {
                        ButtonToggle();
                        Compare_Listbox1.Items.Clear();
                        Compare_Listbox2.Items.Clear();
                        CompareStatus_Box.Text = "Processing...";
                    }
                ));

                // ReSharper disable once CommentTypo
                const int bufferMax = 5 * 1024 * 1024;  // Set to 5MB, max buffer is 2GB: 0x7FFFFFC7
                int bufferCurrent;
                long offsetLarge = 0;
                int index = 0;
                bool sequentialDiff = false;

                FileInfo file1Details = new FileInfo(file1Path);
                if (file1Details.Length < bufferMax)
                {
                    bufferCurrent = (int)file1Details.Length;
                }
                else
                {
                    bufferCurrent = bufferMax;
                }

                while (offsetLarge < file1Details.Length)
                {
                    byte[] file1Buffer = FileReadBuffer(file1Path, offsetLarge, bufferCurrent);
                    byte[] file2Buffer = FileReadBuffer(file2Path, offsetLarge, bufferCurrent);
                    bufferCurrent = file1Buffer.Length;

                    if (bufferCurrent != 0)
                    {
                        int offsetSmall = 0;

                        foreach (byte _ in file1Buffer)
                        {
                            string currentValue1 = BitConverter.ToString(file1Buffer, offsetSmall, 1).Replace("-", string.Empty);
                            string currentValue2 = BitConverter.ToString(file2Buffer, offsetSmall, 1).Replace("-", string.Empty);

                            if (currentValue1 != currentValue2)
                            {
                                if (!sequentialDiff)
                                {
                                    sequentialDiff = true;
                                    string box1 = "0x" + (offsetSmall + offsetLarge).ToString("X") + ": " + currentValue1;
                                    string box2 = "0x" + (offsetSmall + offsetLarge).ToString("X") + ": " + currentValue2;
                                    
                                    Dispatcher.Invoke(new ThreadStart(() =>
                                        {
                                            index = Compare_Listbox1.Items.Add(box1);
                                            Compare_Listbox2.Items.Add(box2);
                                        }
                                    ));
                                }
                                else
                                {
                                    Dispatcher.Invoke(new ThreadStart(() =>
                                        {
                                            ItemEdit(Compare_Listbox1, index, currentValue1);
                                            ItemEdit(Compare_Listbox2, index, currentValue2);
                                        }
                                    ));
                                }
                            }
                            else
                            {
                                sequentialDiff = false;
                            }

                            offsetSmall++;

                            if (offsetSmall == bufferCurrent)
                            {
                                offsetLarge += offsetSmall;
                            }
                        }
                    }
                    
                }

                if (Compare_Listbox1.Items.IsEmpty)
                {
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                            ButtonToggle();
                            CompareStatus_Box.Text = "Files are identical. Time elapsed: " + ElapsedTime(stopWatch);
                        }
                    ));
                }
                else
                {
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                            ButtonToggle();
                            Save_Button.IsEnabled = true;
                            CompareStatus_Box.Text = "Compare completed. Time elapsed: " + ElapsedTime(stopWatch);
                        }
                    ));
                }
            }).Start();
        }

        private void ButtonToggle()
        {
            {
                File1_Button.IsEnabled = !File1_Button.IsEnabled;
                File2_Button.IsEnabled = !File2_Button.IsEnabled;
                Compare_Button.IsEnabled = !Compare_Button.IsEnabled;
            }
        }

        private static void ItemEdit(ListBox box, int index, string text)
        {
            string content = (String)box.Items.GetItemAt(index);
            box.Items.RemoveAt(index);
            box.Items.Insert(index, content + text);
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
                List<string> list = new List<string>();
                ListCreate(list, CompareFile1_Box, Compare_Listbox1);
                ListCreate(list, CompareFile2_Box, Compare_Listbox2);
                WriteFile(list, fileDialog.FileName);
            }
        }

        private static void ListCreate(List<string> list, TextBox box, ListBox listBox)
        {
            list.Add(box.Uid);
            list.Add("-------------");
            list.Add(String.Empty);
            foreach (string s in listBox.Items)
            {
                list.Add(s);
            }

            list.Add(String.Empty);
        }

        private void WriteFile(List<string> list, string path)
        {
            using (TextWriter tw = new StreamWriter(path))
            {
                foreach (String s in list)
                    tw.WriteLine(s);
            }

            if (File.Exists(path))
            {
                CompareStatus_Box.Text = "Saved file: " + path;
            }
            else
            {
                CompareStatus_Box.Text = "Saving failed: Check path write permissions.";
            }
        }

        private string ElapsedTime(Stopwatch stopWatch)
        {
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
            return elapsedTime;
        }
    }
}