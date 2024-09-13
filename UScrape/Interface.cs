

using System;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UScrape
{
    internal class Instruction
    {
        internal enum Types
        {
            Clear,
            Write,
            WriteLine,
            ColorWrite,
            ColorWriteLine,
            SkipLine,
            UpdateLine,
            ReadLine,
            Navigation
        }

        public Types Type { get; }

        public Instruction(Types type)
        {
            Type = type;
        }

        public virtual Instruction Clone()
        {
            return new Instruction(Type);
        }
    }

    internal class Instruction<T> : Instruction
    {
        public T Data { get; }

        public Instruction(Types type, T data) : base(type)
        {
            Data = data;
        }

        public override Instruction<T> Clone()
        {
               return new Instruction<T>(Type, Data);
        }
    }

    public class Interface
    {
        private object Mutex { get; set; }
        private Thread Thread { get; set; }
        private List<Instruction> instructions;
        private List<Instruction> Instructions
        {
            get
            {
                lock (Mutex)
                {
                    return instructions;
                }
            }
            set
            {
                lock (Mutex)
                {
                    instructions = value;
                }
            }
        }

        public bool IsRunning { get; private set; }
        private string header;
        public string Header
        {
            get
            {
                lock (Mutex)
                {
                    return header;
                }
            }
            set
            {
                lock (Mutex)
                {
                    header = value;
                }
            }
        }
        private ConsoleColor color;
        public ConsoleColor Color
        {
            get
            {
                lock (Mutex)
                {
                    return color;
                }
            }
            set
            {
                lock (Mutex)
                {
                    color = value;
                }
            }
        }
        public int HeaderHeight 
        {
            get {
                return Header.Split('\n').Length;
            }
        }

        public Interface(string header, ConsoleColor color, int top)
        {
            Mutex = new object();
            Thread = new Thread(Run);
            Instructions = new List<Instruction>();
            IsRunning = false;
            Header = header;
            Color = color;
        }

        public Interface(string header) : this(header, ConsoleColor.White, 0) { }

        public Interface(ConsoleColor color): this("", color, 0) { }

        public Interface() : this(ConsoleColor.White) { }

        public void Start()
        {
            if (Thread.IsAlive) return;

            Instructions.Clear();
            Clear();

            IsRunning = true;
            Thread = new Thread(Run);
            Thread.Start();
        }

        public void Stop()
        {
            if (!Thread.IsAlive) return;

            IsRunning = false;
            Thread.Join(100);
        }

        private void Run()
        {
            while (IsRunning || Instructions.Count > 0)
            {

                if (Instructions.Count > 0)
                {
                    Instruction instruction;

                    lock (Mutex)
                        instruction = Instructions[0].Clone();

                    Execute(instructions[0]);

                    lock (Mutex)
                        Instructions.RemoveAt(0);
                }
            }
        }

        private void Execute(Instruction instruction)
        {
            if (Console.CursorTop < HeaderHeight - 1)
                Console.SetCursorPosition(0, HeaderHeight - 1);

            switch (instruction.Type)
            {
                case Instruction.Types.Clear:
                    ExecuteClear();
                    break;

                case Instruction.Types.Write:
                    ExecuteWrite(((Instruction<string>)instruction).Data);
                    break;

                case Instruction.Types.WriteLine:
                    ExecuteWriteLine(((Instruction<string>)instruction).Data);
                    break;

                case Instruction.Types.ColorWrite:
                    ExecuteColorWrite(((Instruction<string>)instruction).Data);
                    break;

                case Instruction.Types.ColorWriteLine:
                    ExecuteColorWriteLine(((Instruction<string>)instruction).Data);
                    break;

                case Instruction.Types.SkipLine:
                    ExecuteSkipLine(((Instruction<int>)instruction).Data);
                    break;

                case Instruction.Types.UpdateLine:
                    ExecuteUpdateLine(((Instruction<(string, int)>)instruction).Data.Item1, ((Instruction<(string, int)>)instruction).Data.Item2);
                    break;

                case Instruction.Types.ReadLine:
                    ExecuteReadLine(((Instruction<Action<string>>)instruction).Data);
                    break;

                case Instruction.Types.Navigation:
                    ExecuteNavigation(((Instruction<List<(string, Action)>>)instruction).Data);
                    break;
            }
        }

        private void ExecuteClear()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            ExecuteColorWriteLine(Header);
        }

        private void ExecuteWrite(string value)
        {
            Console.Write(value);
        }

        private void ExecuteWriteLine(string value)
        {
            Console.WriteLine(value);
        }

        private void ExecuteColorWrite(string value)
        {
            Console.ForegroundColor = Color;
            Console.Write(value);
            Console.ResetColor();
        }

        private void ExecuteColorWriteLine(string value)
        {
            Console.ForegroundColor = Color;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        private void ExecuteSkipLine(int count)
        {
            for (int i = 0; i < count; i++)
                Console.WriteLine(new string(' ', Console.WindowWidth));
        }

        private void ExecuteUpdateLine(string value, int top)
        {
            Console.SetCursorPosition(0, top);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, top);
            Console.Write(value);
        }

        private void ExecuteReadLine(Action<string> callback)
        {
            callback(Console.ReadLine() ?? "");
        }

        private void ExecuteNavigation(List<(string, Action)> links)
        {
            // Get the top coordinate of the navigation
            int navigationTop = Console.CursorTop;

            // Print out the links
            for (int i = 0; i < links.Count; i++)
            {
                string link = (links[i].Item1 == "") ? "──" : links[i].Item1;
                link = link.Insert(0, "  ");

                if (i != 0)
                    link = link.Insert(0, "\n");

                if (links[i].Item1 == "")
                    ExecuteColorWrite(link);
                else
                    Console.Write(link);
            }

            // Get the bottom coordinate of the navigation
            int navigationBottom = Console.CursorTop;

            // Set the selector
            for (int i = 0; i < links.Count; i++)
            {
                if (links[i].Item1 == "")
                    continue;

                Console.SetCursorPosition(0, navigationTop + i);
                ExecuteColorWrite(">");

                // Set the cursor at the begginning of the line
                Console.SetCursorPosition(0, navigationTop + i);
                break;
            }

            bool isEnterPressed = false;
            while (!isEnterPressed && Instructions.Count == 1)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    int newCursorTop;

                    switch (key)
                    {
                        case ConsoleKey.UpArrow:

                            newCursorTop = Console.CursorTop;

                            do
                            {
                                newCursorTop--;
                                if (newCursorTop < navigationTop)
                                    newCursorTop = navigationBottom;
                            } while (links[newCursorTop - navigationTop].Item1 == "");

                            // Remove selector
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write(" ");

                            // Rewrite selector
                            Console.SetCursorPosition(0, newCursorTop);
                            ExecuteColorWrite(">");

                            // Set the cursor at the begginning of the line
                            Console.SetCursorPosition(0, newCursorTop);
                            break;
                        case ConsoleKey.DownArrow:

                            newCursorTop = Console.CursorTop;

                            do
                            {
                                newCursorTop++;
                                if (newCursorTop > navigationBottom)
                                    newCursorTop = navigationTop;
                            } while (links[newCursorTop - navigationTop].Item1 == "");

                            // Remove selector
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write(" ");

                            // Rewrite selector
                            Console.SetCursorPosition(0, newCursorTop);
                            ExecuteColorWrite(">");

                            // Set the cursor at the begginning of the line
                            Console.SetCursorPosition(0, newCursorTop);
                            break;
                        case ConsoleKey.Enter:
                            links[Console.CursorTop - navigationTop].Item2();
                            isEnterPressed = true;
                            break;
                    }
                }
            }
        }

        private void AddInstruction(Instruction instruction)
        {
            lock (Mutex)
            {
                if (instruction == null)
                    return;
                Instructions.Add(instruction);
            }
        }

        public void Clear()
        {
            AddInstruction(new Instruction(Instruction.Types.Clear));
        }

        public void Write(string value)
        {
            AddInstruction(new Instruction<string>(Instruction.Types.Write, value));
        }

        public void WriteLine(string value)
        {
            AddInstruction(new Instruction<string>(Instruction.Types.WriteLine, value));
        }

        public void ColorWrite(string value)
        {
            AddInstruction(new Instruction<string>(Instruction.Types.ColorWrite, value));
        }

        public void ColorWriteLine(string value)
        {
            AddInstruction(new Instruction<string>(Instruction.Types.ColorWriteLine, value));
        }

        public void SkipLine(int count = 1)
        {
            AddInstruction(new Instruction<int>(Instruction.Types.SkipLine, count));
        }

        public void UpdateLine(string value, int top)
        {
            AddInstruction(new Instruction<(string, int)>(Instruction.Types.UpdateLine, (value, top)));
        }

        public void ReadLine(Action<string> callback)
        {
            AddInstruction(new Instruction<Action<string>>(Instruction.Types.ReadLine, callback));
        }

        public void ShowNavigation(List<(string, Action)> links)
        {
            AddInstruction(new Instruction<List<(string, Action)>>(Instruction.Types.Navigation, links));
        }
    }
}


