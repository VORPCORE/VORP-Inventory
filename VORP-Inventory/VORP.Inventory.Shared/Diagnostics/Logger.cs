using System;

namespace VORP.Inventory.Shared.Diagnostics
{
    public static class Logger
    {
        static string _loggingLevel = GetResourceMetadata(GetCurrentResourceName(), "log_level", 0);

        static bool ShowOutput(string level)
        {
            string lowercase = _loggingLevel.ToLower();
            if (lowercase == "all") return true;
            return (lowercase == level);
        }

        public static void Info(string msg)
        {
            if (ShowOutput("info"))
                Format($"[INFO] {msg}");
        }

        public static void Trace(string msg)
        {
            if (ShowOutput("trace"))
                Format($"[TRACE] {msg}");
        }

        public static void Warn(string msg)
        {
            if (ShowOutput("warn"))
                Format($"[WARN] {msg}");
        }

        public static void Debug(string msg)
        {
            if (ShowOutput("debug"))
                Format($"[DEBUG] {msg}");
        }

        public static void Error(string msg)
        {
            if (ShowOutput("error"))
                Format($"[ERROR] {msg}");
        }

        /// <summary>
        /// Critical Error does not check the log_level metadata, this will print directly to the console.
        /// </summary>
        /// <param name="msg">Error Message to be displayed, preferably the Method being executed.</param>
        public static void CriticalError(string msg)
        {
            Format($"[CRITICAL_ERROR] {msg}");
        }

        /// <summary>
        /// Critical Error does not check the log_level metadata, this will print directly to the console.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="msg">Error Message to be displayed, preferably the Method being executed.</param>
        public static void CriticalError(Exception ex, string msg)
        {
            Format($"[CRITICAL_ERROR] {msg}\r\n{ex}");
        }

        public static void Error(Exception ex, string msg)
        {
            if (ShowOutput("error"))
                Format($"[ERROR] {msg}\r\n{ex}");
        }

        static void Format(string msg)
        {
            CitizenFX.Core.Debug.WriteLine($"{msg}");
        }
    }
}
