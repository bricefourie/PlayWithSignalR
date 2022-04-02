using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalrtest.Morpion
{
    public static class MorpionHelper
    {
        public const string PLAYER1TOKEN = "X";
        public const string PLAYER2TOKEN = "O";

        public enum GameState
        {
            NotBegin,
            WaitOpponent,
            Play,
            End
        }
    }
}
