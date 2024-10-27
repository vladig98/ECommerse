using System.Text;

namespace UserManagementService.Services
{
    public class LoggingFactory<T>(ILogger<T> logger) : ILoggingFactory<T> where T : class
    {
        // Logging Levels
        private const string LogLevelError = "ERROR";
        private const string LogLevelWarning = "WARN";
        private const string LogLevelInfo = "INFO";
        private const string LogLevelTrace = "TRACE";
        private const string LogLevelDebug = "DEBUG";
        private const string LogLevelCritical = "CRITICAL";

        // Event IDs
        private const int InfoEventId = 0;
        private const int WarnEventId = 1;
        private const int ErrorEventId = 2;
        private const int DebugEventId = 3;
        private const int TraceEventId = 4;
        private const int CriticalEventId = 5;

        // Misc
        private const string LoggingSeparator = "====================";
        private const string LoggingTimeFormat = "yyyy-MMM-dd HH:mm:ss.fff";
        private const string LogeMessageFormatter = "{0} {1}-{2} {0}\r\n{3}\r\n{4}";
        private const string LogMessageFormatString = "{Message}";

        // Messages
        private const string FailedToLogMessage = "Failed to log message";

        private static readonly Action<ILogger, string, Exception?> LogInfoAction =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(InfoEventId, LogLevelInfo), LogMessageFormatString);

        private static readonly Action<ILogger, string, Exception?> LogWarnAction =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(WarnEventId, LogLevelWarning), LogMessageFormatString);

        private static readonly Action<ILogger, string, Exception?> LogErrorAction =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(ErrorEventId, LogLevelError), LogMessageFormatString);

        private static readonly Action<ILogger, string, Exception?> LogDebugAction =
            LoggerMessage.Define<string>(LogLevel.Debug, new EventId(DebugEventId, LogLevelDebug), LogMessageFormatString);

        private static readonly Action<ILogger, string, Exception?> LogTraceAction =
            LoggerMessage.Define<string>(LogLevel.Trace, new EventId(TraceEventId, LogLevelTrace), LogMessageFormatString);

        private static readonly Action<ILogger, string, Exception?> LogCriticalAction =
            LoggerMessage.Define<string>(LogLevel.Critical, new EventId(CriticalEventId, LogLevelCritical), LogMessageFormatString);

        private static readonly CompositeFormat LogMessageFormat = CompositeFormat.Parse(LogeMessageFormatter);

        private readonly ILogger<T> logger = logger;

        public void LogDebug(string header, string message, params string[] messageParams)
        {
            LogMessage(LogDebugAction, LogLevelDebug, header, message, messageParams);
        }

        public void LogError(string header, string message, params string[] messageParams)
        {
            LogMessage(LogErrorAction, LogLevelError, header, message, messageParams);
        }

        public void LogCritical(string header, string message, params string[] messageParams)
        {
            LogMessage(LogCriticalAction, LogLevelCritical, header, message, messageParams);
        }

        public void LogInfo(string header, string message, params string[] messageParams)
        {
            LogMessage(LogInfoAction, LogLevelInfo, header, message, messageParams);
        }

        public void LogTrace(string header, string message, params string[] messageParams)
        {
            LogMessage(LogTraceAction, LogLevelTrace, header, message, messageParams);
        }

        public void LogWarning(string header, string message, params string[] messageParams)
        {
            LogMessage(LogWarnAction, LogLevelWarning, header, message, messageParams);
        }

        private static string GenerateLogMessage(string level, string header, string message)
        {
            return string.Format(null, LogMessageFormat, LoggingSeparator, DateTime.UtcNow.ToString(LoggingTimeFormat), level, header, message);
        }

        private void LogMessage(Action<ILogger, string, Exception?> logAction, string level, string header, string message, params string[] messageParams)
        {
            try
            {
                string logMessage = GenerateLogMessage(level, header, message);
                logMessage = string.Format(logMessage, messageParams);

                logAction(this.logger, logMessage, null);
            }
            catch (Exception ex)
            {
                LogErrorAction(this.logger, FailedToLogMessage, ex);
            }
        }
    }
}
