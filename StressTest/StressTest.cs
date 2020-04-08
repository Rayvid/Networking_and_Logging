using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StressTest
{
    class ConsoleHost
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: StressTest.exe local/remote #");
                return;
            }
            else
            {
                var serviceProvide = new ServiceCollection()
                    .AddLogging((logging) =>
                    {
                        logging.ClearProviders();
                        // Standard way to configure logging, can ommit in this app since we are not using any configuration sources
                        // logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);
                        // TODO our own file logging provider
                    }).BuildServiceProvider();

                var logger = serviceProvide.GetService<ILogger<ConsoleHost>>();

                logger.LogInformation("Starting stress test app");
                Thread.Sleep(10000);
                // TODO further logic
            }
        }
    }
}