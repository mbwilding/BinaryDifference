using System.Collections.Generic;
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
        public List<(string, string, long)> Differences = new();

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
                    Clear();
                    StatusBox.Text = "Processing...";
                });

                var fileStream1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fileStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read, FileShare.Read);
                int bufferSize = FileManager.BufferSize;
                long fileOffset = 0;
                bool difference = false;
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
                        for (int bufferOffset = 0; bufferOffset < buffer1.Length; bufferOffset++)
                        {
                            if (buffer1[bufferOffset] == buffer2[bufferOffset])
                            {
                                difference = false;
                                continue;
                            }

                            string hex1 = ByteToHex(buffer1, bufferOffset);
                            string hex2 = ByteToHex(buffer2, bufferOffset);
                            if (!difference)
                            {
                                Differences.Add((hex1, hex2, fileOffset + bufferOffset));
                            }
                            else
                            {
                                int index = Differences.Count - 1;
                                string hexPrev1 = Differences[index].Item1 + hex1;
                                string hexPrev2 = Differences[index].Item2 + hex2;
                                long offsetPrev = Differences[index].Item3;
                                Differences.RemoveAt(index);
                                Differences.Insert(index, (hexPrev1, hexPrev2, offsetPrev));
                            }
                            difference = true;
                        }
                        fileOffset += bufferSize;
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    File1Button.IsEnabled = true;
                    File2Button.IsEnabled = true;

                    if (Differences.Count != 0)
                    {
                        Format();

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