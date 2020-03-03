using System;
using System.Linq;

namespace CommandLineCalculator.Tests
{
    public sealed class MemoryStorage : Storage
    {
        private byte[] content = Array.Empty<byte>();

        public override byte[] Read()
        {
            return content.ToArray();
        }

        public override void Write(byte[] newContent)
        {
            content = newContent.ToArray();
        }
    }
}