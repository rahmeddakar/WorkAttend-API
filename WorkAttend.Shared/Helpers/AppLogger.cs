using System;
using System.IO;
using System.Runtime.CompilerServices;
using NLog;

namespace WorkAttend.Shared.Helpers
{
    public static class AppLogger
    {
        public static void Info(string message,string action = "",string result = "",string updatedBy = "",string description = "",Exception? exception = null,[CallerMemberName] string methodName = "",[CallerFilePath] string filePath = "")
        {
            WriteLog(LogLevel.Info, message, action, result, updatedBy, description, exception, methodName, filePath);
        }

        public static void Warn(string message,string action = "",string result = "",string updatedBy = "",string description = "",Exception? exception = null,[CallerMemberName] string methodName = "",[CallerFilePath] string filePath = "")
        {
            WriteLog(LogLevel.Warn, message, action, result, updatedBy, description, exception, methodName, filePath);
        }

        public static void Error(string message,string action = "",string result = "",string updatedBy = "",string description = "",Exception? exception = null,[CallerMemberName] string methodName = "",[CallerFilePath] string filePath = "")
        {
            WriteLog(LogLevel.Error, message, action, result, updatedBy, description, exception, methodName, filePath);
        }

        public static void Debug(string message,string action = "",string result = "",string updatedBy = "",string description = "",Exception? exception = null,[CallerMemberName] string methodName = "",[CallerFilePath] string filePath = "")
        {
            WriteLog(LogLevel.Debug, message, action, result, updatedBy, description, exception, methodName, filePath);
        }

        private static void WriteLog(LogLevel level,string message,string action,string result,string updatedBy,string description,Exception? exception,string methodName,string filePath)
        {
            string className = Path.GetFileNameWithoutExtension(filePath);
            var logger = LogManager.GetLogger($"APP.{className}");

            var logEvent = new LogEventInfo(level, className, message);

            logEvent.Properties["CLASS"] = className;
            logEvent.Properties["METHOD"] = methodName;
            logEvent.Properties["UPDATED_BY"] = updatedBy ?? string.Empty;
            logEvent.Properties["ACTION"] = action ?? string.Empty;
            logEvent.Properties["RESULT"] = result ?? string.Empty;
            logEvent.Properties["MESSAGE"] = message ?? string.Empty;
            logEvent.Properties["DESCRIPTION"] = description ?? string.Empty;
            logEvent.Properties["EXCEPTION"] = exception?.ToString() ?? string.Empty;

            logger.Log(logEvent);
        }
    }
}