using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace FileLogger
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// This is the "normal" way to add provider to the list, some DI magic, but in essence it just registers another instance for ILoggerProvider
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomeFileLogProvider>());
            LoggerProviderOptions.RegisterProviderOptions<FileLoggerOptions, CustomeFileLogProvider>(builder.Services);
            return builder;
        }

        /// <summary>
        /// Test/debug scenarios mostly, you shouldnt need to use factories directly
        /// </summary>
        public static ILoggerFactory AddFileLogger(this ILoggerFactory factory)
        {
            factory.AddProvider(new CustomeFileLogProvider());
            return factory;
        }

        /// <summary>
        /// Generic version of constructor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class FileLogger<T> : CustomFileLogger, ILogger<T>
        {
            public FileLogger(CustomeFileLogProvider provider) : base(provider, typeof(T))
            {
            }
        }

        /// <summary>
        /// Shared state for sampling
        /// </summary>
        private static ConcurrentDictionary<object, int> _sampling = new ConcurrentDictionary<object, int>();
        /// <summary>
        /// For scenarios you need to log rarely, but extensively, or it can be sampled at some rate
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="actionToPerformOnLogger">Actual action we want to get sampled</param>
        /// <param name="sampleRateInPercents">What sample rate you want to get (if its > 0, first hit will always be logged, afterwards it will be throttled</param>
        public static void Sampled<T>(this ILogger<T> logger, Action<ILogger<T>> actionToPerformOnLogger, int sampleRateInPercents)
        {
            Sampled(logger, actionToPerformOnLogger, sampleRateInPercents);
        }

        /// <summary>
        /// For scenarios you need to log rarely, but extensively, or it can be sampled at some rate, still full info goes into trace if you need
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="actionToPerformOnLogger">Actual action we want to get sampled</param>
        /// <param name="sampleRateInPercents">What sample rate you want to get (if its > 0, first hit will always be logged, afterwards it will be throttled</param>
        public static void Sampled(this ILogger logger, string message, Action<ILogger, string> actionToPerformOnLogger, int sampleRateInPercents)
        {
            logger.LogTrace(message); // Everything goes to trace level

            if (sampleRateInPercents != 100 && sampleRateInPercents != 0)
            {
                var sample = _sampling.AddOrUpdate(logger, 1, (_caller, old) => old + 1);

                if (sample <= sampleRateInPercents)
                {
                    actionToPerformOnLogger(logger, $"[Sampled at {sampleRateInPercents}%] {message}"); // Add special prefix its sampled
                }

                if (sample == 100)
                {
                    _sampling.AddOrUpdate(logger, 0, (_caller, old) => 0);
                }
            }
        }
    }
}
