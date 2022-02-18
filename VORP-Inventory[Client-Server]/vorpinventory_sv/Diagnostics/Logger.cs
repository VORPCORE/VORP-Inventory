using System;
using static CitizenFX.Core.Native.API;

namespace VorpInventory.Diagnostics
{
    class Logger
    {
        static bool isWarning => GetConvarInt($"vorp_warning_enable", 0) == 1;
        static bool isInfo => GetConvarInt($"vorp_info_enable", 0) == 1;
        static bool isError => GetConvarInt($"vorp_error_enable", 0) == 1;
        static bool isDebug => GetConvarInt($"vorp_debug_enable", 0) == 1;
        static bool isSuccess => GetConvarInt($"vorp_success_enable", 0) == 1;


        public static void Info(string msg)
        {
            if (isInfo)
                WriteLine("INFO", msg);
        }

        public static void Success(string msg)
        {
            if (isSuccess)
                WriteLine("SUCCESS", msg);
        }

        public static void Warn(string msg)
        {
            if (isWarning)
                WriteLine("WARN", msg);
        }

        public static void Error(string msg)
        {
            if (isError)
                WriteLine("ERROR", msg);
        }

        public static void Error(Exception ex, string msg = "")
        {
            if (isError)
                WriteLine("ERROR", $"{msg}\r\n{ex}");
        }

        public static void Debug(string msg)
        {
            bool isDebugging = GetConvarInt($"vorp_debug_enable", 0) == 1;

            if (isDebugging)
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
