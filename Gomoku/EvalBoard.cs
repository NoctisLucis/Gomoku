using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;

namespace Gomoku
{
    class EvalBoard
    {

        public int height, width;
        public int[,] EBoard;

        public EvalBoard()
        {
            height = 12;
            width = 12;
            EBoard = new int[14, 14];
            ResetBoard();
        }

        public void ResetBoard()
        {
            for (int r = 0; r < height + 2; r++)
                for (int c = 0; c < width + 2; c++)
                    EBoard[r, c] = 0;
        }

        public Point MaxPos()
        {
            int Max = 0;
            Point p = new Point();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (EBoard[i, j] > Max)
                    {
                        p.X = i; p.Y = j;
                        Max = EBoard[i, j];
                    }

                }
            }
            return p;
        }

    }
}

