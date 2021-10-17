using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable AccessToModifiedClosure

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        public List<(string, string, long)> Differences = new();

        private async void CheckDifference(string filePath1, string filePath2)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            File1Button.IsEnabled = false;
            File2Button.IsEnabled = false;
            Clear();
            StatusBox.Text = "Processing...";

            var fileStream1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read, FileShare.Read);
            int bufferSize = 1 * 1024 * 1024; // 1MB
            long fileOffset = 0;
            bool disparity = false;
            while (fileOffset < fileStream1.Length)
            {
                var task1 = FileManager.SegmentRead(fileOffset, bufferSize, fileStream1);
                var task2 = FileManager.SegmentRead(fileOffset, bufferSize, fileStream2);
                var buffers = await Task.WhenAll(task1, task2);

                bufferSize = buffers[0].Length;

                if (FileManager.memcmp(buffers[0], buffers[1], bufferSize) != 0)
                {
                    for (int bufferOffset = 0; bufferOffset < buffers[0].Length; bufferOffset++)
                    {
                        if (buffers[0][bufferOffset] == buffers[1][bufferOffset])
                        {
                            disparity = false;
                            continue;
                        }

                        string hex1 = ByteToHex(buffers[0], bufferOffset);
                        string hex2 = ByteToHex(buffers[1], bufferOffset);
                        if (!disparity)
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
                        disparity = true;
                    }
                    fileOffset += bufferSize;
                }
                else
                {
                    disparity = false;
                    fileOffset += bufferSize;
                }
            }

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

            File1Button.IsEnabled = true;
            File2Button.IsEnabled = true;
        }
    }
}