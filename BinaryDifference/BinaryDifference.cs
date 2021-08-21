using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        // ReSharper disable AccessToModifiedClosure
        // ReSharper disable once StringLiteralTypo
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        // ReSharper disable once IdentifierTypo
        private static extern int memcmp(byte[] buffer1, byte[] buffer2, int count);

        private void CheckDifference(string filePath1, string filePath2)
        {
            new Thread(() =>
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                Dispatcher.Invoke(() =>
                {
                    ButtonToggle();
                    Listbox1.Items.Clear();
                    Listbox2.Items.Clear();
                    Status_Box.Text = "Processing...";
                });

                var fileStream1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read);
                var fileStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read);
                int bufferLength = FileManager.BufferSize;
                long fileOffset = 0;
                while (fileOffset < fileStream1.Length)
                {
                    byte[] buffer1 = FileManager.SegmentRead(fileOffset, bufferLength, fileStream1);
                    byte[] buffer2 = FileManager.SegmentRead(fileOffset, bufferLength, fileStream2);
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
                                    string hex1 = ByteToHex(buffer1, bufferOffset);
                                    string hex2 = ByteToHex(buffer2, bufferOffset);
                                    if (bufferOffset != countPrev + 1 || bufferOffset == 0)
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
                                    countPrev = bufferOffset;
                                }
                            }
                            fileOffset += bufferLength;
                        }
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    Finished(stopWatch);
                });
            }).Start();
        }
    }
}