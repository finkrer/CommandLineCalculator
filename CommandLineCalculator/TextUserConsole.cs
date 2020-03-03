using System.IO;

namespace CommandLineCalculator
{
    public sealed class TextUserConsole : UserConsole
    {
        private readonly TextReader input;
        private readonly TextWriter output;

        public TextUserConsole(TextReader input, TextWriter output)
        {
            this.input = input;
            this.output = output;
        }

        public override string ReadLine()
        {
            return input.ReadLine();
        }

        public override void WriteLine(string content)
        {
            output.WriteLine(content);
        }
    }
}