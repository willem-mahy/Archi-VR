using System.Collections.Generic;

namespace WM
{
    public class Logger
    {
        public static List<string> s_log = new List<string>();

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="text"></param>
        public static void Debug(string text)
        {
            s_log.Add(text);

            UnityEngine.Debug.Log(text);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="text"></param>
        public static void Warning(string text)
        {
            s_log.Add("Error: " +text);

            UnityEngine.Debug.LogWarning(text);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="text"></param>
        public static void Error(string text)
        {
            s_log.Add("Warning: " + text);

            UnityEngine.Debug.LogError(text);
        }
    }
}