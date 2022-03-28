using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord.Interactions;
using Discord;
using Newtonsoft.Json;

namespace TetrisBotRewrite
{
    public class Commands : InteractionModuleBase<SocketInteractionContext>
    {
        IConfiguration _config;
        GameManager _gameManager;
        public Commands(IConfiguration configuration, GameManager manager)
        {
            _config = configuration;
            _gameManager = manager;
        }

        [SlashCommand("help", "How to use this bot.")]
        public async Task Help()
        {
            await RespondAsync("Response");
        }

        [SlashCommand("setchannel", "Specify a channel for the Tetris game.")]
        public async Task SetChannel(IMessageChannel channel)
        {
            TetrisGame? game = _gameManager.GetGame(Context.Guild.Id);

            if (game != null)
            {
                game.Channel = channel;
                _gameManager.SaveGame(Context.Guild.Id);
            }
            else
                _gameManager.CreateGame(Context.Guild.Id, channel.Id);


            await RespondAsync("Channel set successfully.");
        }

        [SlashCommand("start", "Starts a game of Tetris in the set channel.")]
        public async Task StartGame()
        {
            TetrisGame? game = _gameManager.GetGame(Context.Guild.Id);
            if (game != null)
            {
                await game.StartGame();
                await RespondAsync($"Tetris game started in #{game.Channel.Name}");
            }
        }

        //[SlashCommand("board", "Show the board.")]
        //public async Task Board()
        //{
        //    var jsonData = File.ReadAllText("data.json");
        //    var guildsList = JsonConvert.DeserializeObject<Dictionary<ulong, TetrisGame>>(jsonData) ?? new Dictionary<ulong, TetrisGame>();
        //    TetrisGame game = guildsList[Context.Guild.Id];

        //    var builder = new ComponentBuilder()
        //        .WithButton("Test", "test-id")
        //        .WithButton("Test1", "test1-id")
        //        .WithButton("Test2", "test2-id")
        //        .WithButton("Test3", "test3-id")
        //        .WithButton("Test4", "test4-id");

        //    await RespondAsync(embed: game.GetEmbed(), components: builder.Build());
        //}
    }
}
