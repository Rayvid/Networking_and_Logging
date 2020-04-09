using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;

namespace FileLogger
{
    [ProviderAlias("FileLogging")] // This is for subsystem to autorecognize config section
    public class CustomFileLogProvider : ILoggerProvider, ISupportExternalScope
    {
        private bool _disposed = false;
        private IExternalScopeProvider _scopeProvider = null;
        private FileLoggerOptions _options;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomFileLogProvider()
        {
            _options = new FileLoggerOptions();
        }

        /// <summary>
        /// Constructor invoked by .net core subsystem
        /// </summary>
        /// <param name="providerOptions">Options passed by .net core subsystem</param>
        public CustomFileLogProvider(IOptionsMonitor<FileLoggerOptions> providerOptions)
        {
            _options = providerOptions.CurrentValue;
            OptionsChanged(_options);

            providerOptions.OnChange(_ => {
                _options = _;
                OptionsChanged(_);
            });
        }

        /// <summary>
        /// Method invoked on configuration options change
        /// </summary>
        /// <param name="options">Options passed by .net core subsystem</param>
        private void OptionsChanged(FileLoggerOptions options)
        {
            if (options.DoReplaceLogFiles)
            {
                Directory.EnumerateFiles(string.Format(options.LogFile, "*")).ToList().ForEach(file =>
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore failure, not fatal
                    }
                });
            }
        }

        /// <summary>
        /// Create logger instance
        /// </summary>
        /// <param name="category">Category of the logger</param>
        /// <returns>Logger instance</returns>
        public ILogger CreateLogger(string category)
        {
            return new CustomFileLogger(this, category);
        }

        /// <summary>
        /// Synchronization object to serialize access to log file
        /// </summary>
        private readonly object _fileSync = new object();
        /// <summary>
        /// Method doing actual logging
        /// </summary>
        /// <param name="logMessage">Log message to log</param>
        internal void DoActualLogging(LogMessage logMessage)
        {
            lock (_fileSync) // This is shared resource, so lock
            {
                try
                {
                    using (var file = File.OpenWrite(String.Format(_options.LogFile, logMessage.Category)))
                    {
                        file.Seek(0, SeekOrigin.End);
                        // TODO format better
                        var contentToAppend = System.Text.UTF8Encoding.UTF8.GetBytes($"{logMessage.Message.ShowTimeAndThread()}\n");
                        file.Write(contentToAppend, 0, contentToAppend.Length);
                        file.Close();
                    }
                }
                catch
                {
                    // Shouldn't be fatal IMO
                }
            }
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        /// <param name="disposing">false - finalizer calling</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Nothing unmanaged
            }

            _disposed = true;
        }

        /// <summary>
        /// To set scope provider externally
        /// </summary>
        /// <param name="scopeProvider">Scope provider to be set</param>
        void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        /// <summary>
        /// To return curr scope provider or autoinitiate new one
        /// </summary>
        internal IExternalScopeProvider ScopeProvider
        {
            get
            {
                if (_scopeProvider == null)
                    _scopeProvider = new LoggerExternalScopeProvider();
                return _scopeProvider;
            }
        }
    }
}
