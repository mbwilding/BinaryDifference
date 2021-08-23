using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable AccessToModifiedClosure

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        private void CheckDifference(string filePath1, string filePath2)
        {
            new Thread(() =>
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                Dispatcher.Invoke(() =>
                {
                    File1Button.IsEnabled = false;
                    File2Button.IsEnabled = false;
                    ListBox1.Items.Clear();
                    ListBox2.Items.Clear();
                    StatusBox.Text = "Processing...";
                });

                var fileStream1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fileStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read, FileShare.Read);
                int bufferSize = FileManager.BufferSize;
                long fileOffset = 0;
                int index = 0;
                while (fileOffset < fileStream1.Length)
                {
                    byte[] buffer1 = FileManager.SegmentRead(fileOffset, bufferSize, fileStream1);
                    byte[] buffer2 = FileManager.SegmentRead(fileOffset, bufferSize, fileStream2);
                    bufferSize = buffer1.Length;

                    if (FileManager.memcmp(buffer1, buffer2, bufferSize) == 0)
                    {
                        fileOffset += bufferSize;
                    }
                    else
                    {
                        int bufferOffsetPrev = -1;
                        for (int bufferOffset = 0; bufferOffset < buffer1.Length; bufferOffset++)
                        {
                            if (buffer1[bufferOffset] == buffer2[bufferOffset]) continue;

                            string hex1 = ByteToHex(buffer1, bufferOffset);
                            string hex2 = ByteToHex(buffer2, bufferOffset);
                            if (bufferOffset != bufferOffsetPrev + 1 || bufferOffset == 0)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    index = SaveFormat(fileOffset, bufferOffset, hex1, hex2, index, true, SaveComboBox.SelectionBoxItem.ToString());
                                });
                            }
                            else
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    SaveFormat(fileOffset, bufferOffset, hex1, hex2, index, false, SaveComboBox.SelectionBoxItem.ToString());
                                });
                            }
                            bufferOffsetPrev = bufferOffset;
                        }
                        fileOffset += bufferSize;
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    File1Button.IsEnabled = true;
                    File2Button.IsEnabled = true;

                    if (!ListBox1.Items.IsEmpty)
                    {
                        switch (SaveComboBox.Text)
                        {
                            case "C# Dictionary":
                                ItemCleanCSharpDictionary(ListBox1, index);
                                ItemCleanCSharpDictionary(ListBox2, index);
                                break;
                        }

                        SaveButton.IsEnabled = true;
                        StatusBox.Text = "Compare completed. Time elapsed: " + ElapsedTime(stopWatch);
                    }
                    else
                    {
                        StatusBox.Text = "Files are identical. Time elapsed: " + ElapsedTime(stopWatch);
                    }
                });
            }).Start();
        }
    }
}