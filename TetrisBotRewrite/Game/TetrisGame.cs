using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;

public enum GameAction
{
    MoveRight,
    MoveLeft,
    MoveDown,
    HardDrop,
    RotateCW,
    RotateCCW,
}

namespace TetrisBotRewrite
{
    public class TetrisGame
    {
        // The channel that the game embeds will be sent to
        public IMessageChannel Channel { get; set; }

        // The current active game embed
        public IUserMessage Message { get; set; }

        // The playing field
        private Grid _grid;

        // The current piece in play
        private Tetromino _currentPiece;

        private int _score;

        private Dictionary<GameAction, int> _votes;

        private List<ulong> _usersVoted;

        private DiscordSocketClient _client;

        private Random _random;

        private System.Timers.Timer _votingTimer = new System.Timers.Timer(30 * 60 * 1000);

        private System.Timers.Timer _updateVoteCountTimer = new System.Timers.Timer(15 * 1000);

        private static Dictionary<GameAction, Emoji> actionEmojis = new Dictionary<GameAction, Emoji>()
        {
            {GameAction.MoveDown, new Emoji("⬇")},
            {GameAction.MoveRight,new Emoji("➡")},
            {GameAction.MoveLeft, new Emoji("⬅")},
            {GameAction.RotateCCW,new Emoji("🔄")},
            {GameAction.RotateCW, new Emoji("🔃")},
            {GameAction.HardDrop, new Emoji("⏬")}
        };


        public TetrisGameData Data
        {
            get
            {
                return new TetrisGameData(Channel.Id, _grid, _currentPiece, _score);
            }
        }

        public TetrisGame(DiscordSocketClient client, ulong channelId)
        {
            _client = client;
            Channel = client.GetChannel(channelId) as IMessageChannel;

            _grid = new Grid();
            _random = new Random();
            _currentPiece = GetNewPiece();
            _score = 0;

            ResetVotes();
            _usersVoted = new List<ulong>();

            InitializeTimers();
        }

        public TetrisGame(DiscordSocketClient client, TetrisGameData data)
        {
            _client = client;
            Channel = client.GetChannel(data.channelId) as IMessageChannel;

            _grid = data.grid;
            _random = new Random();
            _currentPiece = data.currentPiece;
            _score = data.score;

            ResetVotes();
            _usersVoted = new List<ulong>();

            InitializeTimers();
        }

        private void InitializeTimers()
        {
            _votingTimer.AutoReset = true;
            _votingTimer.Elapsed += UpdateGame;
            _votingTimer.Start();

            _updateVoteCountTimer.AutoReset = true;
            _updateVoteCountTimer.Elapsed += UpdateVoteCounts;
            _updateVoteCountTimer.Start();
        }

        public async void UpdateGame(object source, ElapsedEventArgs e)
        {
            // Get the most voted move
            KeyValuePair<GameAction, int> highestVotes = new KeyValuePair<GameAction, int>(GameAction.MoveDown, 0);
            foreach (var pair in _votes)
            {
                if (pair.Value > highestVotes.Value)
                    highestVotes = pair;
            }

            // Don't update, reset timer
            if (highestVotes.Value == 0)
                return;

            GameAction move = highestVotes.Key;

            // Make the move
            ExecuteAction(move);

            // Notify that move was executed
            var builder = new EmbedBuilder();
            builder.WithTitle($"Moving piece {actionEmojis[move]}").WithColor(Color.Blue);
            await Channel.SendMessageAsync("", false, builder.Build());

            // Check line clears
            int lineClears = _grid.CheckLineClears();

            // Send line clear message
            if (lineClears > 0)
            {
                builder = new EmbedBuilder();
                string message;
                switch (lineClears)
                {
                    case 1:
                        _score += 100;
                        message = "Line cleared, +100 points.";
                        break;
                    case 2:
                        _score += 250;
                        message = "Two lines cleared, +250 points.";
                        break;
                    case 3:
                        _score += 525;
                        message = "Three lines cleared, +525 points.";
                        break;
                    case 4:
                        _score += 1000;
                        message = "Tetris! +1000 points.";
                        break;
                    default:
                        message = "";
                        break;
                }
                builder.WithTitle(message)
                       .WithColor(Color.LightOrange);
                await Channel.SendMessageAsync(embed: builder.Build());
            }

            if (_grid.LossCheck())
            {
                await GameOver();
                await RestartGame();
                return;
            }

            // Reset vote stuff
            ResetVotes();
            _usersVoted.Clear();

            // Send board
            await SendEmbed();
        }

        public void ExecuteAction(GameAction action)
        {
            switch (action)
            {
                case GameAction.MoveDown:
                    MoveDown();
                    break;
                case GameAction.MoveRight:
                    MoveRight();
                    break;
                case GameAction.MoveLeft:
                    MoveLeft();
                    break;
                case GameAction.RotateCW:
                    RotateClockwise();
                    break;
                case GameAction.RotateCCW:
                    RotateCounterClockwise();
                    break;
                case GameAction.HardDrop:
                    HardDrop();
                    break;
            }
        }

        public async void UpdateVoteCounts(object source, ElapsedEventArgs e)
        {
            if (Message is null)
                return;

            await Message.ModifyAsync(msg => msg.Embed = GetEmbed());
        }

        public async Task StartGame()
        {
            await SendEmbed();
        }

        public async Task SendEmbed()
        {
            Message = await Channel.SendMessageAsync(embed: GetEmbed(), components: GetButtons());
        }

        public async Task GameOver()
        {
            var builder = new EmbedBuilder();
            builder.WithTitle($"Game over!\n**Final Score: ** {_score}").WithColor(Color.Red).WithCurrentTimestamp();
            var gameOverMessage = await Channel.SendMessageAsync("", false, builder.Build());
            try
            {
                await gameOverMessage.PinAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public async Task RestartGame()
        {
            _grid.ResetGrid();
            _score = 0;
            _currentPiece = GetNewPiece();
            ResetVotes();

            await SendEmbed();
        }

        private Tetromino GetNewPiece()
        {
            TetrominoType type = (TetrominoType)_random.Next(1, 8);
            return new Tetromino(type, new Point(2, 0));
        }

        private void LandCurrentPiece()
        {
            _grid.LandPiece(_currentPiece);
            _currentPiece = GetNewPiece();
        }

        public void MoveRight()
        {
            if (_grid.CanMove(_currentPiece, new Point(1, 0)))
                _currentPiece.position += new Point(1, 0);
        }

        public void MoveLeft()
        {
            if (_grid.CanMove(_currentPiece, new Point(-1, 0)))
                _currentPiece.position += new Point(-1, 0);
        }

        public void MoveDown()
        {
            if (_grid.CanMove(_currentPiece, new Point(0, 1)))
                _currentPiece.position += new Point(0, 1);
            else
                LandCurrentPiece();
        }

        public void HardDrop() // Move the piece down as much as possible then land it
        {
            while (_grid.CanMove(_currentPiece, new Point(0, 1)))
                MoveDown();
            LandCurrentPiece();
        }

        public void RotateClockwise()
        {
            if (_grid.CanRotate(_currentPiece, true))
                _currentPiece = _currentPiece.GetClockwiseRotation();
        }

        public void RotateCounterClockwise()
        {
            if (_grid.CanRotate(_currentPiece, false))
                _currentPiece = _currentPiece.GetClockwiseRotation();
        }

        // Adds the vote and keeps track of the user that voted.
        // If the user already voted it will return false to indicate failure to add vote.
        public bool AddVote(GameAction action, ulong userId)
        {
            if (_usersVoted.Contains(userId))
                return false;
            _votes[action]++;
            _usersVoted.Add(userId);
            return true;
        }

        private void ResetVotes()
        {
            _votes = new Dictionary<GameAction, int>()
            {
                {GameAction.MoveDown, 0},
                {GameAction.MoveRight,0},
                {GameAction.MoveLeft, 0},
                {GameAction.RotateCCW,0},
                {GameAction.RotateCW, 0},
                {GameAction.HardDrop, 0}
            };
        }

        private Embed GetEmbed()
        {
            string votesString = $"⬇: {_votes[GameAction.MoveDown]} ➡: {_votes[GameAction.MoveRight]} ⬅: {_votes[GameAction.MoveLeft]}\n"
                                + $"🔃: {_votes[GameAction.RotateCW]} 🔄: {_votes[GameAction.RotateCCW]} ⏬: {_votes[GameAction.HardDrop]}";
            return new EmbedBuilder()
                .AddField($"Score: {_score}", _grid.GetGridString(_currentPiece), inline: true)
                .AddField("Votes", votesString, false)
                .WithColor(Color.Purple)
                .WithCurrentTimestamp()
                .Build();
        }

        private MessageComponent GetButtons()
        {
            return new ComponentBuilder()
                .WithButton("Down", "vote-move-down", emote: actionEmojis[GameAction.MoveDown])
                .WithButton("Right", "vote-move-right", emote: actionEmojis[GameAction.MoveRight])
                .WithButton("Left", "vote-move-left", emote: actionEmojis[GameAction.MoveLeft])
                .WithButton("Rotate CW", "vote-rotate-cw", emote: actionEmojis[GameAction.RotateCW])
                .WithButton("Rotate CCW", "vote-rotate-ccw", emote: actionEmojis[GameAction.RotateCCW])
                .WithButton("Hard Drop", "vote-hard-drop", emote: actionEmojis[GameAction.HardDrop])
                .Build();
        }
    }
}
