using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisBotRewrite
{
    public class TetrisGameData
    {
        public readonly ulong channelId;
        public readonly Grid grid;
        public readonly Tetromino currentPiece;
        public readonly Tetromino nextPiece;
        public readonly int score;

        public TetrisGameData(ulong channelId, Grid grid, Tetromino currentPiece, Tetromino nextPiece, int score)
        {
            this.channelId = channelId;
            this.grid = grid;
            this.currentPiece = currentPiece;
            this.nextPiece = nextPiece;
            this.score = score;
        }

    }
}
