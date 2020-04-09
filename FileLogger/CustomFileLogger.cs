﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FileLogger
{
    public class FileLoggerOptions
    {
        public string LogFile { get; set; } = "log.txt";
    }

    public class CustomFileLogger : ILogger
    {
        public CustomeFileLogProvider _provider { get; private set; }
        public string _category { get; private set; }

        public CustomFileLogger(CustomeFileLogProvider provider, string category)
        {
            _provider = provider;
            _category = category;
        }

        public CustomFileLogger(CustomeFileLogProvider provider, Type category)
        {
            _provider = provider;
            _category = category.FullName;
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return _provider.ScopeProvider.Push(state);
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return true; // Framework doesnt really use this and we relay on framework to do the filtering
        }

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