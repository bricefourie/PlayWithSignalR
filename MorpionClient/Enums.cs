using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorpionClient
{
    public class Enums
    {
        public enum Methods
        {
            Register,
            Join,
            Turn
        }
        public enum Message
        {
            register,
            error,
            gameId,
            gameState,
            playerToken,
            grille,
            winner
        }
    }
}
