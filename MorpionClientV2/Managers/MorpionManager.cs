using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using MorpionClientV2.Helpers;
using MorpionClientV2.Objects;

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

        public async Task<bool> Register(string username)
        {
            return await _connection.InvokeAsync<bool>(MorpionMessageHelper.register, username);
        }
        public async Task Join()
        {
            await _connection.SendAsync(MorpionMessageHelper.join);
        }
        public async Task<bool> Turn(Guid gameId,int x, int y)
        {
            return await _connection.InvokeAsync<bool>(MorpionMessageHelper.turn, gameId, x, y);
        }
        public async Task Chat(string message)
        {
            await _connection.SendAsync(MorpionMessageHelper.chat, message);
        }
        public async Task<Dictionary<Guid,List<MorpionPlayer>>> Games()
        {
            return await _connection.InvokeAsync<Dictionary<Guid, List<MorpionPlayer>>>(MorpionMessageHelper.games);
        }
        public async Task<bool> Spectate(Guid gameId)
        {
            return await _connection.InvokeAsync<bool>(MorpionMessageHelper.spectate, gameId);
        }
    }
}
