using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EleCho.ConsoleUtilities
{
    public static class ConsoleSc
    {
        static ConsoleSc()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new PlatformNotSupportedException($"{nameof(EleCho)}.{nameof(ConsoleUtilities)} is not supported on non-Windows platforms.");
        }

        private const ConsoleKey readlineUntilKey = ConsoleKey.Enter;

        static bool isReading;
        static bool notIntercept;
        static bool overwriteMode = false;
        static int readIndex = 0;
        static int readHistoryIndex = 0;
        static string? readStr;
        static readonly List<string> readHistory = new List<string>();
        static readonly StringBuilder readBuffer = new StringBuilder();
        static ConsoleKey? readUntil;

        static AutoResetEvent textWriteLock = new AutoResetEvent(true);
        static object textReadLock = new object();

        static string readPrefix = string.Empty;

        static int
            r_startLeft, r_startTop,
            r_cursorLeft, r_cursorTop,
            r_endLeft, r_endTop,
            w_cursorLeft, w_cursorTop,
            w_endLeft, w_endTop;

        public static StringBuilder ReadBuffer => readBuffer;
        public static bool IsReading => isReading;
        public static bool IsOverwriteMode => overwriteMode;
        public static int ReadStartLeft => r_startLeft;
        public static int ReadStartTop => r_startTop;

        public static bool EnablePrompt { get; set; } = true;
        public static bool EnableGlobalization { get; set; } = true;
        public static bool AppendExtraEmptyLineAfterInput { get; set; } = true;

        public static string PromptForInfo { get; set; } = "[#]";
        public static string PromptForWarn { get; set; } = "[!]";
        public static string PromptForError { get; set; } = "[X]";
        public static string PromptForQuestion { get; set; } = "[?]";
        public static string PromptForInput { get; set; } = ">>>";
        public static int TextLengthLimit { get; set; } = 256;
        public static int ExitCode { get; set; } = -1;

        public static CultureInfo CurrentCulture
        {
            get => GlobalizationStrings.CurrentCulture;
            set => GlobalizationStrings.CurrentCulture = value;
        }


        private static string PrefixForInfo = EnablePrompt ? $"{PromptForInfo} " : string.Empty;
        private static string PrefixForWarn = EnablePrompt ? $"{PromptForWarn} " : string.Empty;
        private static string PrefixForError = EnablePrompt ? $"{PromptForError} " : string.Empty;
        private static string PrefixForQuestion = EnablePrompt ? $"{PromptForQuestion} " : string.Empty;
        private static string PrefixForInput = EnablePrompt ? $"{PromptForInput} " : string.Empty;

        private static void SwitchInputHistory(int offset)
        {
            readHistoryIndex += offset;
            int inputHistoryCount = readHistory.Count;
            if (readHistoryIndex < 0)
                readHistoryIndex = 0;
            else if (readHistoryIndex < inputHistoryCount)
            {
                readBuffer.Clear();
                readBuffer.Append(readHistory[readHistoryIndex]);
                readIndex = readBuffer.Length;
            }
            else if (readHistoryIndex >= inputHistoryCount)
            {
                readBuffer.Clear();
                readHistoryIndex = inputHistoryCount;
                readIndex = 0;
            }

        }
        private static void MoveReadIndex(int offset)
        {
            readIndex += offset;
            ReCorrectInputIndex();
        }
        private static void SetReadIndex(int index)
        {
            readIndex = index;
            ReCorrectInputIndex();
        }
        private static void ProcBackspaceKey(bool ctrl = false)
        {
            try
            {
                if (ctrl)
                {
                    readBuffer.Remove(0, readIndex);
                    SetReadIndex(0);
                }
                else
                {
                    readBuffer.Remove(readIndex - 1, 1);
                    MoveReadIndex(-1);
                }
            }
            catch { }
        }
        private static void ProcDeleteKey(bool ctrl = false)
        {
            try
            {
                if (ctrl)
                {
                    readBuffer.Remove(readIndex, readBuffer.Length - readIndex);
                }
                else
                {
                    readBuffer.Remove(readIndex, 1);
                }
            }
            catch { }
        }
        private static void ClearInput()
        {
            readBuffer.Clear();
            SetReadIndex(0);
        }
        private static void FlushInput()
        {
            if (readHistoryIndex == readHistory.Count)
                readHistoryIndex++;

            readStr = readBuffer.ToString();
            readHistory.Add(readStr);

            EndRead();
        }
        private static void BeginRead()
        {
            Monitor.Enter(textReadLock);
            isReading = true;
        }
        private static void EndRead()
        {
            isReading = false;
            Monitor.Exit(textReadLock);
        }
        private static void SetCursorVisible(bool value)
        {
            Console.CursorVisible = value;
        }
        private static void SetCursorSize(int size)
        {
            Console.CursorSize = size;
        }
        private static void RenderReadText()
        {
            TextWriter stdout = Console.Out;
            textWriteLock.WaitOne();

            string before = readBuffer.ToString(0, readIndex);
            string after = readBuffer.ToString(readIndex, readBuffer.Length - readIndex);

            SetCursorVisible(false);
            Console.SetCursorPosition(r_startLeft, r_startTop);

            int
                r_lastEndLeft = r_endLeft,
                    r_lastEndTop = r_endTop;

            stdout.Write(readPrefix);

            if (notIntercept)
                stdout.Write(before);
            r_cursorLeft = Console.CursorLeft;
            r_cursorTop = Console.CursorTop;

            if (notIntercept)
                stdout.Write(after);
            r_endLeft = Console.CursorLeft;
            r_endTop = Console.CursorTop;

            int spaceEx = MeasureSpace(r_endLeft, r_endTop, r_lastEndLeft, r_lastEndTop);

            if (spaceEx > 0)
                stdout.Write(new string(' ', spaceEx));

            if (overwriteMode)
                SetCursorSize(100);
            else
                SetCursorSize(25);

            Console.SetCursorPosition(r_cursorLeft, r_cursorTop);
            SetCursorVisible(true);

            textWriteLock.Set();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>if not append the current char to buffer</returns>
        private static bool DealSpecialKey(ConsoleKeyInfo keyInfo)
        {
            ConsoleModifiers modifiers = keyInfo.Modifiers;
            ConsoleKey key = keyInfo.Key;

            if (!readUntil.HasValue || key.Equals(readUntil))
                FlushInput();
            else
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        SwitchInputHistory(-1);
                        break;
                    case ConsoleKey.DownArrow:
                        SwitchInputHistory(1);
                        break;
                    case ConsoleKey.LeftArrow:
                        MoveReadIndex(-1);
                        break;
                    case ConsoleKey.RightArrow:
                        MoveReadIndex(1);
                        break;
                    case ConsoleKey.Backspace:
                        ProcBackspaceKey(modifiers.PrsControl());
                        break;
                    case ConsoleKey.Delete:
                        ProcDeleteKey(modifiers.PrsControl());
                        break;
                    case ConsoleKey.Insert:
                        overwriteMode ^= true;
                        break;
                    case ConsoleKey.Home:
                        SetReadIndex(0);
                        break;
                    case ConsoleKey.End:
                        SetReadIndex(readBuffer.Length);
                        break;
                    case ConsoleKey.Escape:
                        ClearInput();
                        break;
                    default:
                        return false;
                }

            return true;
        }
        private static void WaitForInput()
        {
            while (true)
            {
                Monitor.Enter(textWriteLock);
                if (!isReading)
                    return;
            }
        }
        private static void PutInputChar(char c)
        {
            if (readBuffer.Length >= TextLengthLimit)
                return;

            if (overwriteMode && readIndex < readBuffer.Length)
                readBuffer[readIndex] = c;
            else
                readBuffer.Insert(readIndex, c);
            MoveReadIndex(1);
        }
        private static int MeasureSpace(int fromLeft, int fromTop, int toLeft, int toTop)
        {
            return Console.BufferWidth * (toTop - fromTop) + (toLeft - fromLeft);
        }

        private static bool PrsControl(this ConsoleModifiers self) => self.HasFlag(ConsoleModifiers.Control);

        private static void ReCorrectInputIndex()
        {
            if (readIndex < 0)
                readIndex = 0;
            if (readIndex > readBuffer.Length)
                readIndex = readBuffer.Length;
        }
        private static string ReadCore(string? message, ConsoleKey until, bool intercept)
        {
            WaitForInput();

            BeginRead();

            readIndex = 0;
            readUntil = until;
            r_startLeft = Console.CursorLeft;
            r_startTop = Console.CursorTop;
            readBuffer.Clear();
            notIntercept = !intercept;

            // 设置输入前缀

            readPrefix = PrefixForInput;

            RenderReadText();
            while (isReading)
            {
                ConsoleKeyInfo readKey = Console.ReadKey(true);
                if (!DealSpecialKey(readKey))
                    PutInputChar(readKey.KeyChar);

                RenderReadText();
            }

            Console.WriteLine();
            return readBuffer.ToString();
        }

        private static ConsoleKeyInfo ReadKeyCore(string message, bool intercept)
        {
            WaitForInput();
            BeginRead();

            readIndex = 0;
            readUntil = null;
            r_startLeft = Console.CursorLeft;
            r_startTop = Console.CursorTop;
            readBuffer.Clear();
            notIntercept = !intercept;

            // 设置输入前缀
                readPrefix = message;


            ConsoleKeyInfo readKey = default;

            RenderReadText();
            while (isReading)
            {
                readKey = Console.ReadKey(true);

                if (!DealSpecialKey(readKey))
                    PutInputChar(readKey.KeyChar);

                RenderReadText();
            }

            Console.WriteLine();
            return readKey;
        }



        public static string ReadLine(bool intercept) => ReadCore(null, readlineUntilKey, intercept);
        public static string ReadLine() => ReadCore(null, readlineUntilKey, false);
        public static int Read() => Console.Read();
        public static char ReadChar() => ReadChar(false);
        public static char ReadChar(bool intercept)
        {
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(intercept);
            }
            while (char.IsControl(keyInfo.KeyChar));
            return keyInfo.KeyChar;
        }

        private static void WriteCore(string text, string? tail, bool renderRead, bool clearAfterEnd)
        {
            TextWriter stdout = Console.Out;

            textWriteLock.WaitOne();                                           // 文本写入锁

            SetCursorVisible(false);                                           // 隐藏光标

            if (isReading)
                Console.SetCursorPosition(r_startLeft, r_startTop);            // 重置光标位置到 ReadLine 时的起始位置
            stdout.Write(text);                                     // 将要写的内容输出

            w_cursorLeft = Console.CursorLeft;                                 // 记录末尾坐标
            w_cursorTop = Console.CursorTop;

            int spaceEx = MeasureSpace(w_cursorLeft, w_cursorTop, r_endLeft, r_endTop);
            if (spaceEx > 0)
                stdout.Write(new string(' ', spaceEx));             // 填充剩余内容

            Console.SetCursorPosition(w_cursorLeft, w_cursorTop);              // 回到记录的末尾坐标

            if (tail != null)
                stdout.Write(tail);

            w_endLeft = Console.CursorLeft;
            w_endTop = Console.CursorTop;

            r_startLeft = w_endLeft;
            r_startTop = w_endTop;

            SetCursorVisible(true);

            textWriteLock.Set();                                               // 释放锁

            if (renderRead && isReading)
                RenderReadText();
        }

        public static void WriteLine(string text, bool renderRead, bool clearAfterEnd) => WriteCore(text, Console.Out.NewLine, renderRead, clearAfterEnd);
        public static void WriteLine(string text, bool renderRead) => WriteCore(text, Console.Out.NewLine, renderRead, false);
        public static void WriteLine(string text) => WriteCore(text, Console.Out.NewLine, true, false);
        public static void WriteLine() => WriteCore(Console.Out.NewLine, null, true, false);
        public static void Write(string text, bool renderRead, bool clearAfterEnd) => WriteCore(text, null, renderRead, clearAfterEnd);
        public static void Write(string text, bool renderRead) => WriteCore(text, null, renderRead, false);
        public static void Write(string text) => WriteCore(text, null, true, false);
        public static ConsoleKeyInfo ReadKey() => Console.ReadKey();
        public static ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);

        public static void PressAnyKeyToContinue(string message)
        {
            ConsoleSc.ReadKeyCore($"{PrefixForInfo}{message}", true);

            if (AppendExtraEmptyLineAfterInput)
                ConsoleSc.WriteLine();
        }

        public static void PressAnyKeyToContinue() =>
            PressAnyKeyToContinue(GlobalizationStrings.PressAnyKeyToContinue);

        public static string ReadFor(string message, string errorMessage, Func<string, bool> validator)
        {
            ConsoleSc.WriteLine($"{PrefixForInfo}{message}");

            while (true)
            {
                string? input =
                    ConsoleSc.ReadCore(message, ConsoleKey.Enter, false);

                if (input == null)
                {
                    Environment.Exit(ExitCode);
                    return string.Empty;
                }

                if (validator.Invoke(input))
                {
                    if (AppendExtraEmptyLineAfterInput)
                        ConsoleSc.WriteLine();

                    return input;
                }

                ConsoleSc.WriteLine($"{PrefixForError}{errorMessage}");
            }
        }

        public static string ReadFor(string message, Func<string, bool> validator) =>
            ReadFor(message, GlobalizationStrings.InvalidInput, validator);

        public static string ReadForString(string message) =>
            ReadFor(message, _ => true);

        public static string ReadForString() =>
            ReadForString(GlobalizationStrings.EnterAString);

        public static int ReadForInt(string message)
        {
            int result = 0;
            ReadFor(message, intstr => int.TryParse(intstr, out result));
            return result;
        }

        public static int ReadForInt() =>
            ReadForInt(GlobalizationStrings.EnterAnInteger);

        public static long ReadForLong(string message)
        {
            long result = 0;
            ReadFor(message, intstr => long.TryParse(intstr, out result));
            return result;
        }

        public static long ReadForLong() =>
            ReadForLong(GlobalizationStrings.EnterAnInteger);

        public static float ReadForFloat(string message)
        {
            float result = 0;
            ReadFor(message, intstr => float.TryParse(intstr, out result));
            return result;
        }

        public static float ReadForFloat() =>
            ReadForFloat(GlobalizationStrings.EnterANumber);

        public static double ReadForDouble(string message)
        {
            double result = 0;
            ReadFor(message, intstr => double.TryParse(intstr, out result));
            return result;
        }

        public static double ReadForDouble() =>
            ReadForDouble(GlobalizationStrings.EnterANumber);

        public static DateTime ReadForDateTime(string message)
        {
            DateTime result = DateTime.Now;
            ReadFor(message, str => DateTime.TryParse(str, out result));
            return result;
        }

        public static DateTime ReadForDateTime() =>
            ReadForDateTime(GlobalizationStrings.EnterADateTime);

        public static TimeSpan ReadForTimeSpan(string message)
        {
            TimeSpan result = TimeSpan.Zero;
            ReadFor(message, str => TimeSpan.TryParse(str, out result));
            return result;
        }

        public static TimeSpan ReadForTimeSpan() =>
            ReadForTimeSpan(GlobalizationStrings.EnterATimeSpan);

        public static int Select(string message, params string[] options)
        {
            ConsoleSc.WriteLine($"{PrefixForQuestion}{message}");
            ConsoleSc.WriteLine($"{PrefixForInfo}{GlobalizationStrings.SelectAnOption}");
            for (int i = 0; i < options.Length; i++)
            {
                ConsoleSc.WriteLine($" {i + 1}. {options[i]}");
            }

            while (true)
            {
                ConsoleSc.Write(PrefixForInput);
                var input =
                    ConsoleSc.ReadLine();

                if (input == null)
                {
                    Environment.Exit(ExitCode);
                    return -1;
                }

                if (int.TryParse(input, out int result))
                {
                    if (result > 0 && result <= options.Length)
                    {
                        if (AppendExtraEmptyLineAfterInput)
                            ConsoleSc.WriteLine();

                        return result - 1;
                    }
                    else
                    {
                        ConsoleSc.WriteLine($"{PrefixForError}{GlobalizationStrings.EnterAnIntegerInSpecifiedRangeToSelectAnOption}");
                    }
                }
                else
                {
                    ConsoleSc.WriteLine($"{PrefixForError}{GlobalizationStrings.EnterAnIntegerToSelectAnOption}");
                }
            }
        }

        public static T Select<T>(string message) where T : struct, Enum
        {
            T[] values =
                Enum.GetValues<T>();

            string[] names = values
                .Select(value => value.ToString())
                .ToArray();

            return values[Select(message, names)];
        }

        public static bool YesOrNo(string message, bool? defaultValue)
        {
            string promptTail;
            if (!defaultValue.HasValue)
                promptTail = "(y/n)";
            else if (defaultValue.Value)
                promptTail = "(Y/n)";
            else
                promptTail = "(y/N)";


            ConsoleSc.Write($"{PrefixForQuestion}{message} {promptTail}");

            bool result;
            while (true)
            {
                var keyInfo =
                    ConsoleSc.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter && defaultValue.HasValue)
                {
                    result = defaultValue.Value;
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Y)
                {
                    result = true;
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.N)
                {
                    result = false;
                    break;
                }

                ConsoleSc.WriteLine($"{PrefixForError}{GlobalizationStrings.InvalidInput}");
            }

            ConsoleSc.WriteLine();

            if (AppendExtraEmptyLineAfterInput)
                ConsoleSc.WriteLine();

            return result;
        }
    }
}
