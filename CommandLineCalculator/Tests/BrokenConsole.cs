namespace CommandLineCalculator.Tests
{
    public sealed class BrokenConsole : UserConsole
    {
        private readonly UserConsole console;
        private readonly int[] failureSchedule;
        private int failureIndex;
        private int actionIndex = -1;

        public BrokenConsole(UserConsole console, int[] failureSchedule)
        {
            this.failureSchedule = failureSchedule;
            this.console = console;
        }

        private int NextBreakIndex => failureIndex >= failureSchedule.Length
            ? int.MaxValue
            : failureSchedule[failureIndex];

        public override string ReadLine()
        {
            FailIfScheduled();
            return console.ReadLine();
        }

        private void FailIfScheduled()
        {
            actionIndex++;
            if (actionIndex == NextBreakIndex)
            {
                failureIndex++;
                throw new TestException("Break", TestException.ExceptionType.InducedFailure);
            }
        }

        public override void WriteLine(string content)
        {
            FailIfScheduled();
            console.WriteLine(content);
        }
    }
}