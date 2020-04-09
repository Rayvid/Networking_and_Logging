using CS3500;
using FileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
                        logging.AddFileLogger();
                        logging.SetMinimumLevel(LogLevel.Debug);
                        // TODO our own file logging provider
                    })
                    .AddTransient<ChatServer>()
                    .AddTransient<ChatClient>()
                    .BuildServiceProvider();

                var logger = serviceProvide.GetService<ILogger<ConsoleHost>>();

                logger.LogInformation("Starting stress test app");
                try
                {
                    bool localServer = args[0] == "local";
                    int stressTestNo = int.Parse(args[1]);

                    var server = serviceProvide.GetService<ChatServer>();
                    if (localServer)
                    {
                        server.StartServer();
                    }

                    var client1 = serviceProvide.GetService<ChatClient>();
                    if (localServer)
                    {
                        client1.StartClient("127.0.0.1", "11000");
                    }
                    else
                    {
                        client1.StartClient(args[2], args[3]);
                    }

                    switch (stressTestNo)
                    {
                        case 1:
                            Task.Run(() => server.SendMessage("Test case 1."));
                            break;

                        case 2:
                            Task.Run(() => server.SendMessage("largemessage"));
                            Task.Run(() => server.SendMessage("largemessage"));
                            Task.Run(() => server.SendMessage("largemessage"));
                            break;
                    }


                    logger.LogInformation("Stress test kicked off - monitor logsfor progress");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Smth wrong have happened when executing stress test");
                }

                Console.WriteLine("Press Enter to exit this app (warning, it will abort test running)");
                Console.Read();
            }
        }
    }
}