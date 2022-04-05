using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorpionClientV2.Helpers
{
    public static class MorpionMessageHelper
    {
        public const string register = "Register";
        public const string join = "Join";
        public const string turn = "Turn";
        public const string error = "error";
        public const string gameId = "gameId";
        public const string gameState = "gameState";
        public const string playerToken = "playerToken";
        public const string grille = "grille";
        public const string winner = "winner";
        public const string info = "info";
        public const string chat = "chat";

        public static string GameStateMessage(int state)
        {
            switch (state)
            {
                case 0:
                    return "Waiting for an opponent...";
                case 1:
                    return "Opponent is playing...";
                case 2:
                    return "It's your turn !";
                case 3:
                    return "Game Over";
                default:
                    return string.Empty;
            }
        }
    }
}
