using System.IO;
using System.Threading.Tasks;

namespace BinaryDifference;

public static class FileManager
{
    public static int MemCmp(byte[] buffer1, byte[] buffer2, int count)
    {
        return System.MemoryExtensions.SequenceEqual(
            new System.ReadOnlySpan<byte>(buffer1, 0, count),
            new System.ReadOnlySpan<byte>(buffer2, 0, count)) ? 0 : 1;
    }

    public static async Task<byte[]> SegmentReadAsync(long offset, int bufferSize, FileStream fileStream)
    {
        if (fileStream.Length - offset < bufferSize)
            bufferSize = (int)(fileStream.Length - offset);

        var buffer = new byte[bufferSize];
        fileStream.Seek(offset, SeekOrigin.Begin);
        await fileStream.ReadExactlyAsync(buffer, 0, bufferSize);
        return buffer;
    }
}
