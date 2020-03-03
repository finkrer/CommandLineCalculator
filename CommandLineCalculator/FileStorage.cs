using System.IO;

namespace CommandLineCalculator
{
    public sealed class FileStorage : Storage
    {
        private readonly string path;

        public FileStorage(string path)
        {
            this.path = path;
        }

        public override byte[] Read()
        {
            using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            using (var buffer = new MemoryStream())
            {
                file.CopyTo(buffer);
                return buffer.ToArray();
            }
        }

        public override void Write(byte[] content)
        {
            using (var file = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.Read))
            {
                file.Write(content, 0, content.Length);
            }
        }
    }
}