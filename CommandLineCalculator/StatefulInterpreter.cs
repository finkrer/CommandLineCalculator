using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CommandLineCalculator
{
    public class Serializer
    {
        private readonly BinaryFormatter formatter = new BinaryFormatter();
        
        public byte[] Serialize(object @object)
        {
            using var memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, @object);
            return memoryStream.ToArray();
        }

        public object Deserialize(byte[] bytes)
        {
            using var memoryStream = new MemoryStream();
            memoryStream.Write(bytes, 0, bytes.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(memoryStream);
        }
    }
    
    [Serializable]
    public class State
    {
        [NonSerialized]
        protected Storage Storage;
        [NonSerialized]
        protected readonly Serializer Serializer = new Serializer();
        public Queue<string> LoadedQueries;
        public Queue<string> QueriesSoFar;
        public int LinesToSkip;
        public int LinesSoFar;
        public long? LastRandomNumber;

        public static State GetFromStorageOrDefault(Storage storage)
        {
            var state = new State(storage);
            if (!state.TryLoadFromStorage())
                state.LoadFromData(new Queue<string>(), 0, null);
            return state;
        }

        protected State(Storage storage) => Storage = storage;

        protected void LoadFromData(Queue<string> loadedQueries, int linesToSkip,
            long? lastRandomNumber)
        {
            LoadedQueries = loadedQueries;
            QueriesSoFar = new Queue<string>(loadedQueries);
            LinesToSkip = linesToSkip;
            LinesSoFar = linesToSkip;
            LastRandomNumber = lastRandomNumber;
        }

        protected void LoadFromState(State other)
        {
            LoadFromData(other.QueriesSoFar, other.LinesSoFar, other.LastRandomNumber);
        }
        
        public void ClearCommand()
        {
            LoadFromData(new Queue<string>(), 0, LastRandomNumber);
            SaveToStorage();
        }

        public void ClearStorage()
        {
            Storage.Write(new byte[0]);
        }

        public void SaveToStorage()
        {
            var bytes = Serializer.Serialize(this);
            Storage.Write(bytes);
        }

        public bool TryLoadFromStorage()
        {
            var bytes = Storage.Read();
            if (bytes.Length <= 0)
                return false;
            LoadFromState((State) Serializer.Deserialize(bytes));
            return true;
        }
    }
    
    public sealed class StatefulUserConsoleWrapper : UserConsole
    {
        private readonly UserConsole console;
        private readonly State state;

        public StatefulUserConsoleWrapper(UserConsole original, State state)
        {
            console = original;
            this.state = state;
        }
        
        public override string ReadLine()
        {
            if (state.LoadedQueries.Count > 0)
                return state.LoadedQueries.Dequeue();
            var query = console.ReadLine();
            state.QueriesSoFar.Enqueue(query);
            state.SaveToStorage();
            return query;
        }

        public override void WriteLine(string content)
        {
            if (state.LinesToSkip > 0)
                state.LinesToSkip--;
            else
            {
                console.WriteLine(content);
                state.LinesSoFar++;
                state.SaveToStorage();
            }
        }
    }
    
    public sealed class StatefulInterpreter : Interpreter
    {
        private static CultureInfo Culture => CultureInfo.InvariantCulture;
        private State state;

        public override void Run(UserConsole userConsole, Storage storage)
        {
            state = State.GetFromStorageOrDefault(storage);
            userConsole = new StatefulUserConsoleWrapper(userConsole, state);
            state.LastRandomNumber ??= 420L;
            while (true)
            {
                var input = userConsole.ReadLine();
                switch (input.Trim())
                {
                    case "exit":
                        state.ClearStorage();
                        return;
                    case "add":
                        Add(userConsole);
                        break;
                    case "median":
                        Median(userConsole);
                        break;
                    case "help":
                        Help(userConsole);
                        break;
                    case "rand":
                        state.LastRandomNumber = Random(userConsole, state.LastRandomNumber.Value);
                        break;
                    default:
                        userConsole.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }
                state.ClearCommand();
            }
        }

        private long Random(UserConsole console, long x)
        {
            const int a = 16807;
            const int m = 2147483647;

            var count = ReadNumber(console);
            for (var i = 0; i < count; i++)
            {
                console.WriteLine(x.ToString(Culture));
                x = a * x % m;
            }

            return x;
        }

        private void Add(UserConsole console)
        {
            var a = ReadNumber(console);
            var b = ReadNumber(console);
            console.WriteLine((a + b).ToString(Culture));
        }

        private void Median(UserConsole console)
        {
            var count = ReadNumber(console);
            var numbers = new List<int>();
            for (var i = 0; i < count; i++)
            {
                numbers.Add(ReadNumber(console));
            }

            var result = CalculateMedian(numbers);
            console.WriteLine(result.ToString(Culture));
        }

        private double CalculateMedian(List<int> numbers)
        {
            numbers.Sort();
            var count = numbers.Count;
            if (count == 0)
                return 0;

            if (count % 2 == 1)
                return numbers[count / 2];

            return (numbers[count / 2 - 1] + numbers[count / 2]) / 2.0;
        }

        private static void Help(UserConsole console)
        {
            const string exitMessage = "Чтобы выйти из режима помощи введите end";
            const string commands = "Доступные команды: add, median, rand";

            console.WriteLine("Укажите команду, для которой хотите посмотреть помощь");
            console.WriteLine(commands);
            console.WriteLine(exitMessage);
            while (true)
            {
                var command = console.ReadLine();
                switch (command.Trim())
                {
                    case "end":
                        return;
                    case "add":
                        console.WriteLine("Вычисляет сумму двух чисел");
                        console.WriteLine(exitMessage);
                        break;
                    case "median":
                        console.WriteLine("Вычисляет медиану списка чисел");
                        console.WriteLine(exitMessage);
                        break;
                    case "rand":
                        console.WriteLine("Генерирует список случайных чисел");
                        console.WriteLine(exitMessage);
                        break;
                    default:
                        console.WriteLine("Такой команды нет");
                        console.WriteLine(commands);
                        console.WriteLine(exitMessage);
                        break;
                }
            }
        }

        private int ReadNumber(UserConsole console)
        {
            return int.Parse(console.ReadLine().Trim(), Culture);
        }
    }
}