using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FileLogger
{
    public class LogMessage
    {
        public string Category { get; private set; }
        public DateTime Timestamp { get; private set; }
        public LogLevel LogLevel { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }
        public IEnumerable<object> Scopes { get; private set; }

        /// <summary>
        /// Its always healthy to have constructor and not allow wild initialization
        /// </summary>
        /// <param name="category">Most frequently theres type name</param>
        /// <param name="logLevel">Information, Debug, Warning, etc</param>
        /// <param name="message">Log message itself</param>
        /// <param name="exception">Exception if any</param>
        /// <param name="scopes">Scopes this log message belongs to</param>
        public LogMessage(
            string category,
            LogLevel logLevel,
            string message,
            Exception exception,
            IEnumerable<object> scopes)
        {
            Category = category;
            Timestamp = DateTime.UtcNow;
            LogLevel = logLevel;
            Message = message;
            Exception = exception;
            Scopes = scopes;
        }
    }
}
