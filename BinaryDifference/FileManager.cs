using System.IO;
using System.Runtime.InteropServices;

namespace BinaryDifference
{
    public static class FileManager
    {
        // ReSharper disable once StringLiteralTypo
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        // ReSharper disable once IdentifierTypo
        internal static extern int memcmp(byte[] buffer1, byte[] buffer2, int count);

        public static readonly int BufferSize = 1 * 1024 * 1024; // 1MB

        public static byte[] SegmentRead(long offset, int bufferSize, FileStream fileStream)
        {
            if (fileStream.Length - offset < bufferSize)
            {
                bufferSize = (int)(fileStream.Length - offset);
            }

            byte[] buffer = new byte[bufferSize];
            _ = fileStream.Seek(offset, SeekOrigin.Begin);
            _ = fileStream.Read(buffer, 0, bufferSize);
            return buffer;
        }
    }
}
