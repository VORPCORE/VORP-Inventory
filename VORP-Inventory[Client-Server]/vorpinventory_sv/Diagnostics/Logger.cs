using System;

namespace VorpInventory.Diagnostics
{
    class Logger
    {
        public static void Info(string msg)
        {
            WriteLine("INFO", msg);
        }

        public static void Success(string msg)
        {
            WriteLine("SUCCESS", msg);
        }

        public static void Warn(string msg)
        {
            WriteLine("WARN", msg);
        }

        public static void Error(string msg)
        {
            WriteLine("ERROR", msg);
        }

        public static void Error(Exception ex, string msg = "")
        {
            WriteLine("ERROR", $"{msg}\r\n{ex}");
        }

        public static void Verbose(string msg)
        {
            WriteLine("VERBOSE", msg);
        }

        public static void Debug(string msg)
        {
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
        }
    }
}
