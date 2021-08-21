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
                    File1_Button.IsEnabled = false;
                    File2_Button.IsEnabled = false;
                    Listbox1.Items.Clear();
                    Listbox2.Items.Clear();
                    Status_Box.Text = "Processing...";
                });

                var fileStream1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fileStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read, FileShare.Read);
                int bufferSize = FileManager.BufferSize;
                long fileOffset = 0;
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
                        int index = 0;
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
                                    index = Listbox1.Items.Add(StringPrepare(fileOffset, bufferOffset, hex1));
                                    Listbox2.Items.Add(StringPrepare(fileOffset, bufferOffset, hex2));
                                });
                            }
                            else
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    ItemEdit(Listbox1, index, hex1);
                                    ItemEdit(Listbox2, index, hex2);
                                });
                            }
                            bufferOffsetPrev = bufferOffset;
                        }
                        fileOffset += bufferSize;
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    File1_Button.IsEnabled = true;
                    File2_Button.IsEnabled = true;

                    if (!Listbox1.Items.IsEmpty)
                    {
                        Save_Button.IsEnabled = true;
                        Status_Box.Text = "Compare completed. Time elapsed: " + ElapsedTime(stopWatch);
                    }
                    else
                    {
                        Status_Box.Text = "Files are identical. Time elapsed: " + ElapsedTime(stopWatch);
                    }
                });
            }).Start();
        }
    }
}