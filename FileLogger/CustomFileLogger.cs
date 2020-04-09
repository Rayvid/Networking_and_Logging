using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FileLogger
{
    public class FileLoggerOptions
    {
        /// <summary>
        /// Log file name pattern
        /// </summary>
        public string LogFile { get; set; } = "Log_{0}.txt";
        /// <summary>
        /// Setting if need to replace old log files
        /// </summary>
        public bool DoReplaceLogFiles { get; set; } = false;
    }

    public class CustomFileLogger : ILogger
    {
        private readonly CustomFileLogProvider _provider;
        private readonly string _category;

        /// <summary>
        /// One of two ways to initialize logger - provider + string category
        /// </summary>
        /// <param name="provider">CustomFileLogProvider instance</param>
        /// <param name="category">Freely defined category</param>
        public CustomFileLogger(CustomFileLogProvider provider, string category)
        {
            _provider = provider;
            _category = category;
        }

        /// <summary>
        /// One of two ways to initialize logger - provider + type category
        /// </summary>
        /// <param name="provider">CustomFileLogProvider instance</param>
        /// <param name="category">Type as a category</param>
        public CustomFileLogger(CustomFileLogProvider provider, Type category)
        {
            _provider = provider;
            _category = category.FullName;
        }

        /// <summary>
        /// Each .net core logging provider should be able accept scopes, of course you can simply ignore those, but better to handle
        /// </summary>
        /// <typeparam name="TState">Scope type</typeparam>
        /// <param name="state">Scope content (in reality its a string most often)</param>
        /// <returns>Disposable to able use scope in using blocks</returns>
        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return _provider.ScopeProvider.Push(state);
        }

        /// <summary>
        /// This is really obsolete, currently its being controlled with filters
        /// </summary>
        /// <param name="logLevel">Info, Warning, Error, etc.</param>
        /// <returns>Value if its enabled or not</returns>
        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return true; // Framework doesnt really use this and we relay on framework to do the filtering
        }

        /// <summary>
        /// Actual logging - we pass everything to the provider to serialize writes into shared resource - file
        /// </summary>
        /// <typeparam name="TState">Log message type (in reality its a string most frequently)</typeparam>
        /// <param name="logLevel">Information, Warning, Error, etc</param>
        /// <param name="eventId">Additional id we ignore</param>
        /// <param name="state">Log message itself</param>
        /// <param name="exception">Exception if any</param>
        /// <param name="formatter">Formater passed by framework, but we prefer our custom formatting logic, so ignore it</param>
        void ILogger.Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var scopes = new List<object>();
            _provider.ScopeProvider.ForEachScope((_, __) => scopes.Add(_), state);
            var logMessage = new LogMessage(_category, logLevel, state.ToString(), exception, scopes);

            _provider.DoActualLogging(logMessage);
        }
    }
}
