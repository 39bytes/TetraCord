using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace TetrisBotRewrite
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup configuration to avoid hard coding environment secrets
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            RunAsync(config).GetAwaiter().GetResult();
        }

        static async Task RunAsync(IConfiguration configuration)
        {
            var clientConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged
            };
            var client = new DiscordSocketClient(clientConfig);

            var gameManager = new GameManager(client);

            // Service collection for dependency injection
            using var services = new ServiceCollection()
                .AddSingleton(configuration)
                .AddSingleton(client)
                .AddSingleton(gameManager)
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();

            var commands = services.GetRequiredService<InteractionService>();

            client.Log += LogAsync;

            // Only load game data after guild data has finished downloading
            client.Ready += async () => await gameManager.LoadGames();

            commands.Log += LogAsync;

            // Global commands can take a while to register, but guild commands update instantly.
            // For debugging purposes, we can specify a specific test guild to test commands quickly.
            client.Ready += async () =>
            {
                if (IsDebug())
                {
                    // Id of the test guild can be provided from the Configuration object
                    await commands.RegisterCommandsToGuildAsync(configuration.GetValue<ulong>("testGuild"), true);
                }
                else
                    await commands.RegisterCommandsGloballyAsync(true);
            };

            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            // Login using the bot token provided in the configuration file
            await client.LoginAsync(TokenType.Bot, configuration["token"]);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        static Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
             return false;
#endif
        }
    }
}
