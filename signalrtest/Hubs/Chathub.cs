using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using signalrtest.Morpion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalrtest.Hubs
{
    public static class UserHandler
    {
        public static int ConnectedClients = 0;
    }
    public class Chathub : Hub
    {
        private readonly IMorpionManager _morpionManager;
        private readonly ILogger _logger;

        public Chathub(ILogger<Chathub> logger, IMorpionManager morpionManager)
        {
            _logger = logger;
            _morpionManager = morpionManager;
        }
        public override async Task OnConnectedAsync()
        {
            UserHandler.ConnectedClients++;
            _logger.LogInformation($"Received a new Connection , number of clients : {UserHandler.ConnectedClients}");
            await Clients.Caller.SendAsync("newmsg","Server", "Welcome to Chathub");
            await Clients.AllExcept(Context.ConnectionId).SendAsync("newmsg", "Server", "A New Challenger enter in the ring!");
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            UserHandler.ConnectedClients--;
            _logger.LogInformation($"Loosing a Connection, number of clients : {UserHandler.ConnectedClients}");
            if(exception is not null)
            {
                _logger.LogError("Exception on disconnection : {error}",exception.Message);
            }
            return base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(string username,string message)
        {
            _logger.LogInformation($"Chat from {username}");
            await Clients.All.SendAsync("newmsg", username, message);
        }
    }
}
