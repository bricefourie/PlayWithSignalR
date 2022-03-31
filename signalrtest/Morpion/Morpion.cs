using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalrtest.Morpion
{
    public class Morpion
    {
        private string[,] _grille = new string[3, 3];
        private int _lastplayer = 0;
        public string[,] grille
        {
            get
            {
                return _grille;
            }
        }
        private const string PLAYER1TOKEN = "X";
        private const string PLAYER2TOKEN = "O";

        private bool CanPlayHere(int x, int y)
        {
            return string.IsNullOrWhiteSpace(_grille[x, y]);
        }

        private bool IsPlayerTurn(int player)
        {
            return player != _lastplayer;
        }
        private bool IsGameOver()
        {
            if (Winner() != 0)
            {
                return true;
            }
            int fullLine = 0;
            for (int i = 0; i < 3; i++)
            {
                if (!string.IsNullOrWhiteSpace(_grille[i, 0]) && !string.IsNullOrWhiteSpace(_grille[i, 1]) && !string.IsNullOrWhiteSpace(_grille[i, 2]))
                {
                    fullLine++;
                }
            }
            return fullLine == 3;
        }

        public int Winner()
        {
            int winner = 0;
            for (int i = 0; i < _grille.GetLength(0); i++)
            {
                if (AllAreEqual(GetColumn(_grille, i)) && !string.IsNullOrWhiteSpace(_grille[0,i]))
                {
                    return TokenToPlayer(_grille[0, i]);
                }
                if (AllAreEqual(GetRow(_grille, i)) && !string.IsNullOrWhiteSpace(_grille[i,0]))
                {
                    return TokenToPlayer(_grille[i, 0]);
                }
            }
            if(AllAreEqual(GetDiag(_grille)) || AllAreEqual(GetDiag2(_grille)))
            {
                return TokenToPlayer(_grille[1, 1]);
            }

            return winner;
        }

        public string[] GetColumn(string[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }

        public string[] GetRow(string[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

        public string[] GetDiag(string[,] matrix)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                .Select(x => matrix[x, x])
                .ToArray();
        }
        public string[] GetDiag2(string[,] matrix)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                .Select(x => matrix[x,matrix.GetLength(0)-x])
                .ToArray();
        }

        private bool AllAreEqual(string[] args)
        {
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] != args[i - 1])
                    return false;
            }
            return true;
        }
        private int TokenToPlayer(string token)
        {
            switch (token)
            {
                case PLAYER1TOKEN:
                    return 1;
                case PLAYER2TOKEN:
                    return 1;
                default:
                    return 0;
            }
        }

        public bool SetToken(int x, int y, int player)
        {
            if(IsGameOver() || !IsPlayerTurn(player) )
            {
                return false;
            }
            var token = string.Empty;
            switch (player)
            {
                case 1:
                    token = PLAYER1TOKEN;
                    break;
                case 2:
                    token = PLAYER2TOKEN;
                    break;
                default:
                    return false;
            }
            if (CanPlayHere(x, y))
            {
                _grille[x, y] = token;
                return _grille[x, y] == token;
            }
            return false;
        }


    }
}
