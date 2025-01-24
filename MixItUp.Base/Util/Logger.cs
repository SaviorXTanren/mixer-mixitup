using System;

namespace MixItUp.Base.Util
{
    /// <summary>
    /// The level of importance of a log
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// A fatal, crashing log
        /// </summary>
        Fatal = 0,
        /// <summary>
        /// An error log
        /// </summary>
        Error = 10,
        /// <summary>
        /// A warning log
        /// </summary>
        Warning = 20,
        /// <summary>
        /// An informational log
        /// </summary>
        Information = 30,
        /// <summary>
        /// A debug log
        /// </summary>
        Debug = 40
    }

    /// <summary>
    /// A log for the application
    /// </summary>
    public class Log
    {
        /// <summary>
        /// The level of the log
        /// </summary>
        public LogLevel Level { get; set; }
        /// <summary>
        /// The log message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates a new instance of the Log class.
        /// </summary>
        public Log() { }

        /// <summary>
        /// Creates a new instance of the Log class.
        /// </summary>
        /// <param name="level">The level of the log</param>
        /// <param name="message">The log message</param>
        public Log(LogLevel level, string message)
        {
            this.Level = level;
            this.Message = message;
        }
    }

    /// <summary>
    /// Handles the logging for information.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Sets the maximum log level to invoke the event method for.
        /// </summary>
        public static LogLevel Level { get; private set; } = LogLevel.Warning;

        /// <summary>
        /// Invoked when a log occurrs.
        /// </summary>
        public static event EventHandler<Log> LogOccurred = delegate { };

        /// <summary>
        /// Sets the maximum log level to invoke the event method for.
        /// </summary>
        /// <param name="level">The maximum level to capture logs for</param>
        public static void SetLogLevel(LogLevel level) { Logger.Level = level; }

        /// <summary>
        /// Indicates where to always log full stack trace when exceptions are logged.
        /// </summary>
        public static bool AlwaysLogFullStackTraceWithExceptions { get; set; }

        /// <summary>
        /// Logs the specified message at the Information level.
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Log(string message)
        {
            Logger.Log(LogLevel.Information, message);
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="level">The level of the log</param>
        /// <param name="message">The message to log</param>
        public static void Log(LogLevel level, string message)
        {
            try
            {
                if (level <= Logger.Level)
                {
                    Logger.LogOccurred(null, new Log(level, message));
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Logs the specified exception at the Error level.
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="includeStackTrace">Whether to include a full stack trace</param>
        public static void Log(Exception ex, bool includeStackTrace = false)
        {
            Logger.Log(LogLevel.Error, ex, includeStackTrace);
        }

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="level">The level of the log</param>
        /// <param name="ex">The exception to log</param>
        /// <param name="includeStackTrace">Whether to include a full stack trace</param>
        public static void Log(LogLevel level, Exception ex, bool includeStackTrace = false)
        {
            string log = ex.ToString();
            if (AlwaysLogFullStackTraceWithExceptions || includeStackTrace)
            {
                log += Environment.NewLine + "Full Stack:" + Environment.StackTrace;
            }
            Logger.Log(level, log);
        }

        /// <summary>
        /// Forcefully logs the specified message.
        /// </summary>
        /// <param name="level">The level of the log</param>
        /// <param name="message">The message to log</param>
        public static void ForceLog(LogLevel level, string message)
        {
            try
            {
                Logger.LogOccurred(null, new Log(level, message));
            }
            catch (Exception) { }
        }
    }
}
