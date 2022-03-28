using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace TetrisBotRewrite
{
    public class GameManager
    {
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(
            new ConfigurationOptions
            {
                EndPoints = { "localhost:6379" }
            });

        // Dictionary containing all guilds and their associated Tetris game
        private Dictionary<ulong, TetrisGame> games;

        // Discord client is required to be passed to each Tetris game
        private DiscordSocketClient _client;

        public GameManager(DiscordSocketClient client)
        {
            _client = client;
            games = new Dictionary<ulong, TetrisGame>();
            _client.ButtonExecuted += ButtonHandler;
        }

        // Gets the game associated to a guild if it exists
        public TetrisGame? GetGame(ulong guildId) => games.GetValueOrDefault(guildId);

        public async Task LoadGames()
        {
            var db = redis.GetDatabase();

            foreach (string id in redis.GetServer("localhost:6379").Keys()) // Get all guild ids
            {
                // Get the game data for the guild
                string json = await db.StringGetAsync(id);
                TetrisGameData data = JsonConvert.DeserializeObject<TetrisGameData>(json);

                if (data == null)
                {
                    throw new Exception($"Game data for {id} loaded from Redis was null.");
                }

                // Load it into the dictionary
                TetrisGame game = new TetrisGame(_client, data);
                games.Add(ulong.Parse(id), game);
            }
        }

        public void CreateGame(ulong guildId, ulong channelId)
        {
            var db = redis.GetDatabase();
            TetrisGame game = new TetrisGame(_client, channelId);
            string serialized = JsonConvert.SerializeObject(game.Data);
            db.StringSet(guildId.ToString(), serialized);
            games.Add(guildId, game);

            Console.WriteLine($"Created Tetris game for {guildId}");
        }

        public void SaveGame(ulong guildId)
        {
            var db = redis.GetDatabase();
            TetrisGame game = games[guildId];
            string serialized = JsonConvert.SerializeObject(game.Data);
            db.StringSet(guildId.ToString(), serialized);

            Console.WriteLine($"Saved Tetris game for {guildId}");
        }

        public async Task ButtonHandler(SocketMessageComponent component)
        {
            // The guild that the button was clicked in
            var channel = component.Channel as SocketGuildChannel;
            if (channel == null)
                return;

            ulong guildId = channel.Guild.Id;

            TetrisGame? game = GetGame(guildId);
            if (game == null)
                return;

            if (game.Message is null)
                return;

            if (game.Message.Id != component.Message.Id)
                return;

            ulong userId = component.User.Id;

            switch (component.Data.CustomId)
            {
                case "vote-move-down":
                    await component.RespondAsync("You voted move down.");
                    game.AddVote(GameAction.MoveDown, userId);
                    break;
                case "vote-move-right":
                    await component.RespondAsync("You voted move right.");
                    game.AddVote(GameAction.MoveRight, userId);
                    break;
                case "vote-move-left":
                    await component.RespondAsync("You voted move left.");
                    game.AddVote(GameAction.MoveLeft, userId);
                    break;
                case "vote-rotate-cw":
                    await component.RespondAsync("You voted rotate clockwise.");
                    game.AddVote(GameAction.RotateCW, userId);
                    break;
                case "vote-rotate-ccw":
                    await component.RespondAsync("You voted rotate counter-clockwise.");
                    game.AddVote(GameAction.RotateCCW, userId);
                    break;
                case "vote-hard-drop":
                    await component.RespondAsync("You voted hard drop.");
                    game.AddVote(GameAction.HardDrop, userId);
                    break;
            }
        }
    }
}
