using System.IO;

namespace BinaryDifference
{
    public static class FileManager
    {
        public static readonly int BufferSize = 4 * 1024 * 1024; // 4MB

        public static byte[] SegmentRead(long offset, int bufferSize, FileStream fileStream)
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
}
