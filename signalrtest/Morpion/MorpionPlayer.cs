using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalrtest.Morpion
{
    public class MorpionPlayer
    {
        private string clientId;

        public string ClientId
        {
            get { return clientId; }
        }

        private string userName;

        public string Username
        {
            get { return userName; }
            set { userName = value; }
        }

        public MorpionPlayer(string ClientId, string Username)
        {
            clientId = ClientId;
            userName = Username;
        }
    }
}
