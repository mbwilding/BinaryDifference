using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
                string path1 = File1_Box.Uid;
                string path2 = File2_Box.Uid;
                Task.Run(() => CheckDifference(path1, path2));
            }
            else
            {
                Status_Box.Text = "Files cannot be different sizes.";
            }
        }

        private void CheckDifference(string filePath1, string filePath2)
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

            // for loop eventually
            Task<Tuple<List<string>, List<string>>> task1 = Task.Factory.StartNew(() => ThreadProcess(fileStream1, fileStream2, 0, bufferLength));
            Task<Tuple<List<string>, List<string>>> task2 = Task.Factory.StartNew(() => ThreadProcess(fileStream1, fileStream2, bufferLength, bufferLength));
            //Task<Tuple<List<string>, List<string>>> task3 = Task.Factory.StartNew(() => ThreadProcess(fileStream1, fileStream2, bufferLength * 2, bufferLength));
            //Task<Tuple<List<string>, List<string>>> task4 = Task.Factory.StartNew(() => ThreadProcess(fileStream1, fileStream2, bufferLength * 3, bufferLength));
            task1.Wait();
            Debug.WriteLine("Thread 1 complete");
            task2.Wait();
            Debug.WriteLine("Thread 2 complete");

            //Task.WaitAll(task1, task2/*, task3*/);
            //Debug.WriteLine("All threads complete");

            /*
            var finalList1 = task1.Result.Item1
                //.Concat(task2.Result.Item1)
                //.Concat(task3.Result.Item1)
                .ToList();
            var finalList2 = task1.Result.Item2
                //.Concat(task2.Result.Item2)
                //.Concat(task3.Result.Item2)
                .ToList();
            */
            


            Dispatcher.Invoke(new ThreadStart(() =>
                {
                    foreach (string s in task1.Result.Item1)
                    {
                        Listbox1.Items.Add(s);
                    }
                    foreach (string s in task1.Result.Item2)
                    {
                        Listbox2.Items.Add(s);
                    }

                    foreach (string s in task2.Result.Item1)
                    {
                        Listbox1.Items.Add(s);
                    }
                    foreach (string s in task2.Result.Item2)
                    {
                        Listbox2.Items.Add(s);
                    }
                }
            ));

            //fileStream1.Dispose();
            //fileStream2.Dispose();

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

            // TODO TEMPORARY BYPASS
            return bufferThreshold;

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

        private static string StringPrepare(long fileOffset, int bufferOffset, string value)
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

        private static Tuple <List<string>, List<string>> ThreadProcess(FileStream fileStream1, FileStream fileStream2, long fileOffset, int bufferLength)
        {
            List<string> list1 = new();
            List<string> list2 = new();

            byte[] buffer1 = FileReadBuffer(fileOffset, bufferLength, fileStream1);
            byte[] buffer2 = FileReadBuffer(fileOffset, bufferLength, fileStream2);
            bufferLength = buffer1.Length;

            if (bufferLength != 0)
            {
                if (memcmp(buffer1, buffer2, bufferLength) == 0)
                {
                    return Tuple.Create(list1, list2);
                }

                int bufferOffsetPrevious = -1;
                for (int bufferOffset = 0; bufferOffset < buffer1.Length; bufferOffset++)
                {
                    if (buffer1[bufferOffset] != buffer2[bufferOffset])
                    {
                        string value1 = BitConverter.ToString(buffer1, bufferOffset, 1);
                        string value2 = BitConverter.ToString(buffer2, bufferOffset, 1);

                        if (bufferOffset != bufferOffsetPrevious + 1 || bufferOffset == 0)
                        {
                            list1.Add(StringPrepare(fileOffset, bufferOffset, value1));
                            list2.Add(StringPrepare(fileOffset, bufferOffset, value2));
                            //Debug.WriteLine("list1: " + value1);
                            //Debug.WriteLine("list2: " + value2);
                        }
                        else
                        {
                            int position = list1.Count - 1;
                            string previousString = list1[position];
                            list1[position] = previousString + value1;
                            previousString = list2[position];
                            list2[position] = previousString + value2;
                            //Debug.WriteLine("list1: " + value1);
                            //Debug.WriteLine("list2: " + value2);
                        }
                        bufferOffsetPrevious = bufferOffset;
                    }
                }
            }
            return Tuple.Create(list1, list2);
        }
    }
}