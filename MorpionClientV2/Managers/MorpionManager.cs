using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using MorpionClientV2.Helpers;

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

        public async Task Register(string username)
        {
            await _connection.SendAsync(MorpionMessageHelper.register, username);
        }
        public async Task Join()
        {
            await _connection.SendAsync(MorpionMessageHelper.join);
        }
        public async Task<bool> Turn(Guid gameId,int x, int y)
        {
            return await _connection.InvokeAsync<bool>(MorpionMessageHelper.turn, gameId, x, y);
        }
    }
}
