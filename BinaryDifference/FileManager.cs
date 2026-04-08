using System.IO;
using System.Threading.Tasks;

namespace BinaryDifference
{
    public static class FileManager
    {
        /// <summary>
        /// Cross-platform replacement for the native msvcrt memcmp.
        /// Returns 0 if the two spans are equal, non-zero otherwise.
        /// </summary>
        public static int MemCmp(byte[] buffer1, byte[] buffer2, int count)
        {
            return System.MemoryExtensions.SequenceEqual(
                new System.ReadOnlySpan<byte>(buffer1, 0, count),
                new System.ReadOnlySpan<byte>(buffer2, 0, count)) ? 0 : 1;
        }

        public static async Task<byte[]> SegmentRead(long offset, int bufferSize, FileStream fileStream)
        {
            if (fileStream.Length - offset < bufferSize)
            {
                bufferSize = (int)(fileStream.Length - offset);
            }

            byte[] buffer = new byte[bufferSize];
            fileStream.Seek(offset, SeekOrigin.Begin);
            await fileStream.ReadExactlyAsync(buffer, 0, bufferSize);
            return buffer;
        }
    }
}
