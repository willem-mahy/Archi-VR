using System.Collections.Generic;

namespace WM
{
    public class Logger
    {
        public static List<string> s_log = new List<string>();

        public static bool Enabled = false;

        /// <summary>
        /// Enables/Disables the logger.
        /// </summary>
        /// <param name="state"></param>
        public static void SetEnabled(bool state)
        {
            Enabled = state;
        }

        /// <summary>
        /// Clears the log.
        /// </summary>
        /// <param name="text"></param>
        public static void Clear()
        {
            s_log.Clear();
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="text"></param>
        public static void Debug(string text)
        {
            if (!Enabled)
                return;

            s_log.Add(text);

            UnityEngine.Debug.Log(text);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="text"></param>
        public static void Warning(string text)
        {
            if (!Enabled)
                return;

            s_log.Add("Error: " +text);

            UnityEngine.Debug.LogWarning(text);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="text"></param>
        public static void Error(string text)
        {
            if (!Enabled)
                return;

            s_log.Add("Warning: " + text);

            UnityEngine.Debug.LogError(text);
        }
    }
}