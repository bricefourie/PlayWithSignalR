using Microsoft.AspNetCore.SignalR;
using signalrtest.Morpion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalrtest.Hubs
{
    public static class MorpionPlayerHandler
    {
        public static HashSet<MorpionPlayer> Players = new HashSet<MorpionPlayer>();
    }
    public class MorpionHub : Hub
    {
        private readonly IMorpionManager _morpionManager;
        public MorpionHub(IMorpionManager morpionManager)
        {
            _morpionManager = morpionManager;
        }
        public async Task Register(string username)
        {
            if (!MorpionPlayerHandler.Players.Any(x => x.ClientId == Context.ConnectionId))
            {
                MorpionPlayerHandler.Players.Add(new MorpionPlayer(Context.ConnectionId, username));
                await Clients.Client(Context.ConnectionId).SendAsync("register", true);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync("register", false);
            }
        }
        public async Task Join()
        {
            var player = MorpionPlayerHandler.Players.FirstOrDefault(x => x.ClientId == Context.ConnectionId);
            if (player is null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("error", "Please register before join");
            }
            else
            {
                var gameId =_morpionManager.JoinAGame(player);
                if(gameId.Equals(Guid.Empty))
                {
                    gameId = _morpionManager.NewMorpionGame(player);
                    await Clients.Client(player.ClientId).SendAsync("gameId", gameId);
                    await Clients.Client(Context.ConnectionId).SendAsync("gameState", MorpionHelper.GameState.NotBegin);
                }
                else
                {
                    var players = _morpionManager.GetMorpionPlayers(gameId);
                    await Clients.Client(players[0].ClientId).SendAsync("playerToken",MorpionHelper.PLAYER1TOKEN);
                    await Clients.Client(players[1].ClientId).SendAsync("playerToken",MorpionHelper.PLAYER2TOKEN);
                    await Clients.Clients(players.Select(x => x.ClientId)).SendAsync("grille", _morpionManager.GetGrille(gameId));
                    await Clients.Client(players[0].ClientId).SendAsync("gameState", MorpionHelper.GameState.Play);
                    await Clients.Client(player.ClientId).SendAsync("gameState", MorpionHelper.GameState.WaitOpponent);
                    
                }

            }
        }

        public async Task Turn(Guid gameId,int x, int y)
        {
            var players = _morpionManager.GetMorpionPlayers(gameId);
            if(_morpionManager.PlayerTurn(gameId,players.FirstOrDefault(x => x.ClientId == Context.ConnectionId),x,y))
            {
                if(!_morpionManager.IsGameOver(gameId))
                {
                    await Clients.Clients(players.Select(x => x.ClientId)).SendAsync("grille", _morpionManager.GetGrille(gameId));
                    await Clients.Client(Context.ConnectionId).SendAsync("gameState", MorpionHelper.GameState.WaitOpponent);
                    await Clients.Client(players.FirstOrDefault(x => x.ClientId != Context.ConnectionId).ClientId).SendAsync("gameState", MorpionHelper.GameState.Play);
                }
                else
                {
                    await Clients.Clients(players.Select(x => x.ClientId)).SendAsync("gameState", MorpionHelper.GameState.End);
                    await Clients.Clients(players.Select(x => x.ClientId)).SendAsync("winner", _morpionManager.WinnerOfTheGame(gameId).Username);
                    // When the game is over -> remove the game
                }
            }
        }

    }
}
