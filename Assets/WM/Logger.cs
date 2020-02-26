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
        /// Whether UDP network communications should be logged.
        /// </summary>
        public bool enableLogUDP = false;

        public enum LogType
        {
            Debug = 1,
            Warning = 2,
            Error = 4
        }

        /// <summary>
        /// 
        /// </summary>
        public class LogEntry
        {
            public readonly LogType LogType;

            public readonly string Text;

            public LogEntry(
                LogType logType,
                string text)
            {
                LogType = logType;
                Text = text;
            }
        }

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
        /// Gets the log entry at given index.
        /// </summary>
        public LogEntry this[int lineIndex]
        {
            get => _log[lineIndex];
        }

        /// <summary>
        /// Gets the number of lines in the log.
        /// </summary>
        public int NumEntries
        {
            get { return _log.Count; }
        }

        /// <summary>
        /// Gets the number of Debug entries in the log.
        /// </summary>
        public int NumDebugEntries
        {
            get { return _numDebugEntries; }
        }

        /// <summary>
        /// Gets the number of Warning entries in the log.
        /// </summary>
        public int NumWarningEntries
        {
            get { return _numWarningEntries; }
        }

        /// <summary>
        /// Gets the number of Error entries in the log.
        /// </summary>
        public int NumErrorEntries
        {
            get { return _numErrorEntries; }
        }

        /// <summary>
        /// Clears the log.
        /// </summary>
        /// <param name="text"></param>
        public void Clear()
        {
            _log.Clear();
            _numDebugEntries = _numWarningEntries = _numErrorEntries = 0;
        }

        /// <summary>
        /// Adds a Debug entry in the log, formatted as a header.
        /// </summary>
        public void Header(string caption)
        {
            Debug("=======[" + caption + "]===============================");
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

            _log.Add(new LogEntry(LogType.Debug, text));

            UnityEngine.Debug.Log(text);

            ++_numDebugEntries;
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

            _log.Add(new LogEntry(LogType.Warning, text));

            UnityEngine.Debug.LogWarning(text);

            ++_numWarningEntries;
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

            _log.Add(new LogEntry(LogType.Error, text));

            UnityEngine.Debug.LogError(text);

            ++_numErrorEntries;
        }

        #endregion Public API

        #region Fields

        private int _numDebugEntries = 0;

        private int _numWarningEntries = 0;

        private int _numErrorEntries = 0;

        /// <summary>
        /// The logged lines.
        /// </summary>
        private readonly List<LogEntry> _log = new List<LogEntry>();

        #endregion Fields
    }
}