using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace FileLogger
{
    [ProviderAlias("FileLogging")] // This is for subsystem to autorecognize config section
    public class CustomeFileLogProvider : ILoggerProvider, ISupportExternalScope
    {
        private bool _disposed = false;
        private IExternalScopeProvider _scopeProvider = null;
        private FileLoggerOptions _options;

        public CustomeFileLogProvider()
        {
            _options = new FileLoggerOptions();
        }

        public CustomeFileLogProvider(IOptionsMonitor<FileLoggerOptions> providerOptions) // Default options
        {
            _options = providerOptions.CurrentValue;
            providerOptions.OnChange(_ => {
                _options = _;
            });
        }

        public ILogger CreateLogger(string category)
        {
            return new CustomFileLogger(this, category);
        }

        private object _fileSync = new object();
        internal void DoActualLogging(LogMessage logMessage)
        {
            lock (_fileSync) // This is shared resource, so lock
            {
                using (var file = File.OpenWrite(_options.LogFile))
                {
                    file.Seek(0, SeekOrigin.End);
                    // TODO format better
                    var contentToAppend = System.Text.UTF8Encoding.UTF8.GetBytes($"{logMessage.Timestamp} {logMessage.Message}\n");
                    file.Write(contentToAppend, 0, contentToAppend.Length);
                    file.Close();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

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
