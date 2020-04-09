using CS3500;
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
                Console.WriteLine("usage: StressTest.exe local/remote # [remote address] [remote port]");
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

                var server = new ChatServer();
                server.StartServer();

                var client = new ChatClient();
                client.StartClient("127.0.0.1", "11000");

                Task.Run(() =>server.SendMessage("largemessage"));

                // TODO further logic and remove sleep
                Thread.Sleep(10000);
            }
        }
    }
}