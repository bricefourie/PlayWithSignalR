using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalrtest.Morpion
{
    public static class MorpionMessageHelper
    {
        public const string register = "register";
        public const string error = "error";
        public const string gameId = "gameId";
        public const string gameState = "gameState";
        public const string playerToken = "playerToken";
        public const string grille = "grille";
        public const string winner = "winner";
    }
}
