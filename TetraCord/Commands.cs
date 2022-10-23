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
            var builder = new EmbedBuilder();
            builder.WithTitle("Help")
                   .AddField("/setchannel", "Sets the channel where the game will be played.")
                   .AddField("/start", "Starts the game, you only need to do this after setting the channel for the first time")
                   .AddField("General Help", "Vote on the move to make by clicking the buttons! You only get one move per turn.")
                   .WithColor(Color.Purple);
            await RespondAsync(embed: builder.Build());
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
            else{
                await RespondAsync("You must first specify the channel to play the game in with /setchannel.");
            }
        }
    }
}
