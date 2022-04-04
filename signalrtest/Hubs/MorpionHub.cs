using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;
        public MorpionHub(IMorpionManager morpionManager,
            ILogger<MorpionHub> logger)
        {
            _morpionManager = morpionManager;
            _logger = logger;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("{player} disconnected", MorpionPlayerHandler.Players.FirstOrDefault(x => x.ClientId == Context.ConnectionId).Username);
            MorpionPlayerHandler.Players.RemoveWhere(x => x.ClientId == Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
        public async Task Register(string username)
        {
            if (!MorpionPlayerHandler.Players.Any(x => x.ClientId == Context.ConnectionId))
            {
                MorpionPlayerHandler.Players.Add(new MorpionPlayer(Context.ConnectionId, username));
                await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.register, true);
                _logger.LogInformation("New challenger come in the ring : {0}", username);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.register, false);
                _logger.LogError("Registration failed, already exist {0}",MorpionPlayerHandler.Players.FirstOrDefault(x => x.ClientId == Context.ConnectionId).Username);
            }
        }
        public async Task Join()
        {
            var player = MorpionPlayerHandler.Players.FirstOrDefault(x => x.ClientId == Context.ConnectionId);
            if (player is null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.error, "Please register before join");
            }
            else
            {
                var gameId =_morpionManager.JoinAGame(player);
                if(gameId.Equals(Guid.Empty))
                {
                    gameId = _morpionManager.NewMorpionGame(player);
                    await Clients.Client(player.ClientId).SendAsync(MorpionMessageHelper.gameId, gameId);
                    await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.NotBegin);
                    _logger.LogInformation("New game created, GameId {gameId}", gameId);
                }
                else
                {
                    var players = _morpionManager.GetMorpionPlayers(gameId);
                    await Clients.Client(player.ClientId).SendAsync(MorpionMessageHelper.gameId, gameId);
                    await Clients.Client(players[0].ClientId).SendAsync(MorpionMessageHelper.playerToken,MorpionHelper.PLAYER1TOKEN);
                    await Clients.Client(players[1].ClientId).SendAsync(MorpionMessageHelper.playerToken,MorpionHelper.PLAYER2TOKEN);
                    await Clients.Clients(players.Select(x => x.ClientId)).SendAsync(MorpionMessageHelper.grille, _morpionManager.GetGrille(gameId));
                    await Clients.Client(players[0].ClientId).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.Play);
                    await Clients.Client(player.ClientId).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.WaitOpponent);
                    _logger.LogInformation("A Game begin, gameId : {gameId} with Player1 : {player1} and Player2: {player2}",gameId,players[0].Username,players[1].Username);
                }

            }
        }

        public async Task<bool> Turn(Guid gameId,int x, int y)
        {
            var players = _morpionManager.GetMorpionPlayers(gameId);
            var player = players.FirstOrDefault(x => x.ClientId == Context.ConnectionId);
            var opponent = players.FirstOrDefault(x => x != player);
            var value = false;
            if (_morpionManager.PlayerTurn(gameId, player, x, y))
            {
                await Clients.Clients(players.Select(x => x.ClientId)).SendAsync(MorpionMessageHelper.grille, _morpionManager.GetGrille(gameId));
                if (!_morpionManager.IsGameOver(gameId))
                {
                    await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.WaitOpponent);
                    await Clients.Client(opponent.ClientId).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.Play);
                }
                else
                {
                    await Clients.Clients(players.Select(x => x.ClientId)).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.End);
                    await Clients.Clients(players.Select(x => x.ClientId)).SendAsync(MorpionMessageHelper.winner, _morpionManager.WinnerOfTheGame(gameId).Username);
                    _morpionManager.DeleteGame(gameId);
                }
                value = true;
            }
            else
            {
                await Clients.Client(player.ClientId).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.Play);
                await Clients.Client(player.ClientId).SendAsync(MorpionMessageHelper.error, $"You can't play in {x}:{y}");
            }
            return value;
        }
    }
}
