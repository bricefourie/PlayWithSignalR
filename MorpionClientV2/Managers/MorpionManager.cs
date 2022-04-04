using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace MorpionClientV2.Managers
{
    public class MorpionManager
    {
        private readonly HubConnection _connection;
        public HubConnection HubConnection
        {
            get { return _connection; }
        }
        public MorpionManager()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/Morpion")
                .AddNewtonsoftJsonProtocol()
                .Build();
        }
    }
}
