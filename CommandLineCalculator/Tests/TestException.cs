using System;

namespace CommandLineCalculator.Tests
{
    public sealed class TestException : Exception
    {
        public ExceptionType Type { get; }

        public TestException(string message, ExceptionType type)
            : base(message)
        {
            Type = type;
        }

        public enum ExceptionType
        {
            WrongRead,
            WrongWrite,
            InducedFailure
        }
    }
}