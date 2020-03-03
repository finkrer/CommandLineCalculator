namespace CommandLineCalculator
{
    public abstract class Storage
    {
        public abstract byte[] Read();
        public abstract void Write(byte[] content);
    }
}