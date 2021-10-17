using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// ReSharper disable once StringLiteralTypo
// ReSharper disable once IdentifierTypo

namespace BinaryDifference
{
    public static class FileManager
    {
        
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int memcmp(byte[] buffer1, byte[] buffer2, int count);

        public static async Task<byte[]> SegmentRead(long offset, int bufferSize, FileStream fileStream)
        {
            if (fileStream.Length - offset < bufferSize)
            {
                bufferSize = (int)(fileStream.Length - offset);
            }

            byte[] buffer = new byte[bufferSize];
            fileStream.Seek(offset, SeekOrigin.Begin);
            await fileStream.ReadAsync(buffer, 0, bufferSize);
            return buffer;
        }
    }
}
