using System;

namespace VorpInventory.Diagnostics
{
    class Logger
    {
        public static void Info(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            WriteLine("INFO", msg);
        }

        public static void Success(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine("SUCCESS", msg);
        }

        public static void Warn(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLine("WARN", msg);
        }

        public static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine("ERROR", msg);
        }

        public static void Error(Exception ex, string msg = "")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine("ERROR", $"{msg}\r\n{ex}");
        }

        public static void Verbose(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            WriteLine("VERBOSE", msg);
        }

        public static void Debug(string msg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            WriteLine("DEBUG", msg);
        }

        private static void WriteLine(string title, string msg)
        {
            try
            {
                var m = $"[{title}] {msg}";
                CitizenFX.Core.Debug.WriteLine($"{m}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ResetColor();
        }
    }
}