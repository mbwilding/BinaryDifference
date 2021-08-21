using System.Diagnostics;
using System.IO;
using BinaryDifference;

namespace BinaryDifference
{
    public class Buffer
    {
        public static byte[] SegmentRead(FileManager fileManager, long offset)
        {
            if (fileManager.FileLength - offset < fileManager.BufferSize)
            {
                fileManager.BufferSize = (int)(fileManager.FileLength - offset);

                if (fileManager.BufferSize <= 0)
                {
                    return null;
                }
            }

            byte[] buffer = new byte[fileManager.BufferSize];
            _ = fileManager.FileStream.Seek(offset, SeekOrigin.Begin);
            _ = fileManager.FileStream.Read(buffer, 0, fileManager.BufferSize);
            return buffer;
        }

        public static int SetBufferSize(long fileSize)
        {
            const int bufferThreshold = 5 * 1024 * 1024;
            const int bufferMax = 2000 * 1024 * 1024;
            const int bufferDivisions = 4; // 60 is optimal for 2GB files

            Debug.WriteLine("BufferSize: " + (int)fileSize / bufferDivisions);
            return (int)(fileSize / bufferDivisions);

            /*if (fileSize / bufferDivisions > bufferMax)
            {
                Debug.WriteLine("BufferSize [1]: " + bufferMax);
                return bufferMax;
            }
            if (fileSize > bufferThreshold * bufferDivisions)
            {
                Debug.WriteLine("BufferSize [2]: " + (int)fileSize / bufferDivisions);
                return (int)fileSize / bufferDivisions;
            }
            Debug.WriteLine("BufferSize [3]: " + (int)fileSize);
            return (int)fileSize;*/
        }
    }
}
