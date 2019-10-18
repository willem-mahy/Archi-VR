using System.Collections.Generic;

namespace WM
{
    public class Logger
    {
        public static List<string> s_log = new List<string>();

        //! Logs a debug message.
        public static void Debug(string text)
        {
            s_log.Add(text);

            UnityEngine.Debug.Log(text);
        }

        //! Logs a warning message.
        public static void Warning(string text)
        {
            s_log.Add("Error: " +text);

            UnityEngine.Debug.LogWarning(text);
        }

        //! Logs an error message.
        public static void Error(string text)
        {
            s_log.Add("Warning: " + text);

            UnityEngine.Debug.LogError(text);
        }
    }
}