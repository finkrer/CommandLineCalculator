using System;
using System.Linq;

namespace CommandLineCalculator.Tests
{
    public class TestConsole : UserConsole
    {
        public enum Action : byte
        {
            Read,
            Write
        }

        private readonly (Action Action, string Value)[] schedule;
        private int position;

        public TestConsole(params (Action Action, string Value)[] schedule)
        {
            this.schedule = schedule;
        }

        public override string ReadLine()
        {
            if (position >= schedule.Length)
            {
                throw new TestException(
                    "Attempt to read at end",
                    TestException.ExceptionType.WrongRead
                );
            }

            var (action, value) = schedule[position];
            if (action != Action.Read)
            {
                var state = PrintError("READ");
                throw new TestException(
                    $"Attempt to read when write expected.\n{state}",
                    TestException.ExceptionType.WrongRead
                );
            }

            position++;
            return value;
        }

        public override void WriteLine(string content)
        {
            if (position >= schedule.Length)
            {
                throw new TestException(
                    "Attempt to write at end",
                    TestException.ExceptionType.WrongWrite
                );
            }

            var (action, value) = schedule[position];
            if (action != Action.Write)
            {
                var state = PrintError($"WRITE: {content}");
                throw new TestException(
                    $"Attempt to write when read expected.\n{state}",
                    TestException.ExceptionType.WrongWrite
                );
            }

            if (!string.Equals(value, content, StringComparison.InvariantCulture))
            {
                var state = PrintError($"WRITE: {content}");
                throw new TestException(
                    $"Attempt to write not what expected.\n{state}",
                    TestException.ExceptionType.WrongWrite
                );
            }

            position++;
        }

        public bool AtEnd => schedule.Length == position;
        public override string ToString() => Print("\n>\n");
        private string PrintError(string action) => Print($"\n> WRONG ATTEMPT: {action} <\n");
        private string Print(string inner)
        {
            var prefix = string.Join("\n", schedule.Take(position).Select(Print));
            var suffix = string.Join("\n", schedule.Skip(position).Select(Print));
            return $"{prefix}{inner}{suffix}";

            string Print((Action Action, string Value) entry)
            {
                var (action, value) = entry;
                return action == Action.Read
                    ? $"READ : {value}"
                    : $"WRITE: {value}";
            }
        }
    }
}