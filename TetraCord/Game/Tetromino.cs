﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum TetrominoType
{
    T = 1,
    L = 2,
    J = 3,
    S = 4,
    Z = 5,
    I = 6,
    O = 7,
}

namespace TetrisBotRewrite
{
    public class Tetromino
    {
        public readonly TetrominoType type;
        // The top left of the piece
        public Point position;

        public int[] shape;
        public int currentRotation = 0;

        public Tetromino(TetrominoType type, Point position, int rotation = 0)
        {
            this.type = type;
            int[][] rotations = shapes[type];
            this.position = position;
            currentRotation = rotation;
            shape = rotations[currentRotation];
        }

        // Removes the padded space around the 4x4 shape matrix
        // i don't know a simpler way to do this 
        public override string ToString()
        {
            var points = ShapeToPoints().Select(p => p = p - position);
            int[,] tiles = new int[4, 4];

            foreach (Point p in points)
            {
                tiles[p.y, p.x] = (int)type;
            }

            // Mark empty rows
            for (int row = 0; row < 4; row++)
            {
                bool empty = true;
                for (int col = 0; col < 4; col++)
                {
                    if (tiles[col, row] != 0 && tiles[col, row] != -1)
                        empty = false;
                }
                if (empty)
                {
                    for (int col = 0; col < 4; col++)
                    {
                        tiles[col, row] = -1;
                    }
                }
            }

            // Mark empty columns
            for (int col = 0; col < 4; col++)
            {
                bool empty = true;
                for (int row = 0; row < 4; row++)
                {
                    if (tiles[col, row] != 0 && tiles[col, row] != -1)
                        empty = false;
                }
                if (empty)
                {
                    for (int row = 0; row < 4; row++)
                    {
                        tiles[col, row] = -1;
                    }
                }
            }

            // Convert to string
            string output = "";
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    if (tiles[row, col] != -1)
                        output += Utils.TetrominoTypeToEmoji(tiles[row, col]);
                }
                output += "\n";
            }

            return output;
        }

        // Convert the tile indices into 2d coordinates (ex: 6 becomes (2, 1))
        public Point[] ShapeToPoints()
        {
            Point[] points = new Point[4];
            for (int i = 0; i < 4; i++)
            {
                int x = position.x + shape[i] % 4;
                int y = position.y + shape[i] / 4;
                points[i] = new Point(x, y);
            }
            return points;
        }

        public Tetromino GetClockwiseRotation()
        {
            int newRotation = currentRotation == 3 ? 0 : currentRotation + 1;
            return new Tetromino(type, position, newRotation);
        }

        public Tetromino GetCounterClockwiseRotation()
        {
            int newRotation = currentRotation == 0 ? 3 : currentRotation - 1;
            return new Tetromino(type, position, newRotation);
        }

        // Positions of the tiles on a 4x4 matrix
        /* [0, 1, 2, 3,
            4, 5, 6, 7,
            8, 9, 10, 11,
            12, 13, 14, 15] */
        private static readonly Dictionary<TetrominoType, int[][]> shapes = new Dictionary<TetrominoType, int[][]>()
        {
            { TetrominoType.T, new int[][]{ new int[] { 1, 4, 5, 6}, new int[] { 1, 5, 6, 9 }, new int[] { 4, 5, 6, 9 }, new int[] { 1, 4, 5, 9 } } },
            { TetrominoType.L, new int[][]{ new int[] { 2, 4, 5, 6}, new int[] { 1, 5, 9, 10 }, new int[] { 4, 5, 6, 8 }, new int[] { 0, 1, 5, 9 } }},
            { TetrominoType.J, new int[][]{ new int[] { 0, 4, 5, 6}, new int[] { 1, 2, 5, 9 }, new int[] { 4, 5, 6, 10 }, new int[] { 1, 5, 8, 9 } }},
            { TetrominoType.S, new int[][]{ new int[] { 1, 2, 4, 5}, new int[] { 1, 5, 6, 10 }, new int[] { 5, 6, 8, 9 }, new int[] { 0, 4, 5, 9 } }},
            { TetrominoType.Z, new int[][]{ new int[] { 0, 1, 5, 6}, new int[] { 2, 5, 6, 9 }, new int[] { 4, 5, 9, 10 }, new int[] { 1, 4, 5, 8 } }},
            { TetrominoType.I, new int[][]{ new int[] { 4, 5, 6, 7}, new int[] { 2, 6, 10, 14 }, new int[] { 8, 9, 10, 11 }, new int[] { 1, 5, 9, 13 } }},
            { TetrominoType.O, new int[][]{ new int[] { 1, 2, 5, 6}, new int[] { 1, 2, 5, 6 }, new int[] { 1, 2, 5, 6 }, new int[] { 1, 2, 5, 6 } } },
        };
    }
}
