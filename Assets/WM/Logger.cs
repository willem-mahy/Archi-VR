using System.Collections.Generic;

namespace WM
{
    /// <summary>
    /// A log where error, warning and info messages can be logged to.
    /// </summary>
    public class Logger
    {
        #region Public API

        /// <summary>
        /// Enabled state of the Logger.
        /// If not enabled, calls to LogXXX() are NOOP.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Gets the line at given index.
        /// </summary>
        public string this[int lineIndex]
        {
            get => _log[lineIndex];
        }

        /// <summary>
        /// Gets the number of lines in the log.
        /// </summary>
        public int Count
        {
            get { return _log.Count; }
        }

        /// <summary>
        /// Clears the log.
        /// </summary>
        /// <param name="text"></param>
        public void Clear()
        {
            _log.Clear();
        }

        /// <summary>
        /// Logs a header message.
        /// </summary>
        /// <param name="caption"></param>
        public void Header(string caption)
        {
            _log.Add("");
            _log.Add("=======[" + caption + "]===============================");
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="text"></param>
        public void Debug(string text)
        {
            if (!Enabled)
            {
                return;
            }

            _log.Add(text);

            UnityEngine.Debug.Log(text);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="text"></param>
        public void Warning(string text)
        {
            if (!Enabled)
            {
                return;
            }

            _log.Add("Error: " +text);

            UnityEngine.Debug.LogWarning(text);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="text"></param>
        public void Error(string text)
        {
            if (!Enabled)
            {
                return;
            }

            _log.Add("Warning: " + text);

            UnityEngine.Debug.LogError(text);
        }

        #endregion Public API

        #region Fields

        /// <summary>
        /// The logged lines.
        /// </summary>
        private readonly List<string> _log = new List<string>();

        #endregion Fields
    }
}