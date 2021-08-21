using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BinaryDifference
{
    class ThreadWorker
    {
        // ReSharper disable once StringLiteralTypo
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        // ReSharper disable once IdentifierTypo
        private static extern int memcmp(byte[] buffer1, byte[] buffer2, int count);

        public Tuple<List<string>, List<string>> ThreadProcess(FileManager fileStream1, FileManager fileStream2, long fileOffset)
        {
            List<string> list1 = new();
            List<string> list2 = new();

            byte[] buffer1 = Buffer.SegmentRead(fileStream1, fileOffset);
            byte[] buffer2 = Buffer.SegmentRead(fileStream2, fileOffset);

            if (buffer1.Length != 0)
            {
                if (memcmp(buffer1, buffer2, buffer1.Length) == 0)
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
                        }
                        else
                        {
                            int position = list1.Count - 1;

                            list1[position] += value1;
                            list2[position] += value2;
                        }
                        bufferOffsetPrevious = bufferOffset;
                    }
                }
            }
            return Tuple.Create(list1, list2);
        }
        
        private static string StringPrepare(long fileOffset, int bufferOffset, string value)
        {
            return "0x" + (fileOffset + bufferOffset).ToString("X") + ": " + value;
        }
    }
}
