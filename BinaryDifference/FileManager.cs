using System.IO;

namespace BinaryDifference
{
    public class FileManager
    {
        public string FilePath { get; }

        public long FileLength { get; }

        public FileStream FileStream { get; }

        public FileManager(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                this.FilePath = filePath;
                FileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                FileLength = FileStream.Length;
            }
        }
    }
}