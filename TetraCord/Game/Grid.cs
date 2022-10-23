using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisBotRewrite
{
    public class Grid
    {
        const int Height = 12;
        const int Width = 8;
        public int[,] landed = new int[Height, Width];

        public void ResetGrid()
        {
            landed = new int[Height, Width];
        }

        public bool CanMove(Tetromino piece, Point direction)
        {
            Point[] currentPosition = piece.ShapeToPoints();
            IEnumerable<Point> newPosition = currentPosition.Select(p => p + direction);

            return IsColliding(newPosition);
        }

        public bool CanRotate(Tetromino piece, bool clockwise)
        {
            Tetromino rotated;
            if (clockwise)
                rotated = piece.GetClockwiseRotation();
            else
                rotated = piece.GetCounterClockwiseRotation();

            return IsColliding(rotated.ShapeToPoints());
        }

        public bool IsColliding(IEnumerable<Point> points)
        {
            foreach (Point p in points)
            {
                if (p.x < 0 || p.x >= Width) // Piece is outside the sides of the field
                    return false;
                if (p.y >= Height) // Piece is past the bottom
                    return false;
                if (landed[p.y, p.x] != 0) // Piece hit an existing tile
                    return false;
            }
            return true;
        }

        public void LandPiece(Tetromino piece)
        {
            foreach (Point p in piece.ShapeToPoints())
            {
                landed[p.y, p.x] = (int)piece.type;
            }
        }

        public int CheckLineClears()
        {
            List<int> clearedLines = new List<int>();
            bool isFilled;
            for (int row = 0; row < Height; row++)
            {
                isFilled = true;
                for (int col = 0; col < Width; col++)
                {
                    if (landed[row, col] == 0)
                        isFilled = false;
                }

                //empty the row
                if (isFilled)
                {
                    clearedLines.Add(row);
                    for (int n = 0; n < landed.GetLength(1); n++)
                        landed[row, n] = 0;
                }
            }

            int numLinesCleared = clearedLines.Count;

            while (clearedLines.Any())
            {
                // Pop the last cleared line from the list
                int line = clearedLines.Last();
                clearedLines.RemoveAt(clearedLines.Count - 1);

                // Move everything above that line down
                for (int col = 0; col < Width; col++)
                {
                    for (int row = line; row > 0; row--)
                    {
                        landed[row, col] = landed[row - 1, col];
                    }
                    landed[0, col] = 0;
                }

                // Move the empty line indices down as well
                for (int i = 0; i < clearedLines.Count; i++)
                {
                    clearedLines[i]++;
                }
            }

            return numLinesCleared;
        }

        public bool LossCheck()
        {
            return (landed[0, 3] != 0 || landed[0, 4] != 0);
        }

        public string GetGridString(Tetromino currentPiece)
        {
            string result = "";

            var piecePoints = currentPiece.ShapeToPoints();

            // Draw the grid
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (piecePoints.Contains(new Point(x, y)))
                        result += Utils.TetrominoTypeToEmoji((int)currentPiece.type);
                    else
                        result += Utils.TetrominoTypeToEmoji(landed[y, x]);
                }
                result += "\n";
            }

            return result;
        }
    }
}
