﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using signalrtest.Morpion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalrtest.Hubs
{
    public static class MorpionPlayerHandler
    {
        public static HashSet<MorpionPlayer> Players = new HashSet<MorpionPlayer>();
        public static ConcurrentDictionary<Guid, ConcurrentBag<MorpionPlayer>> SpectateGames = new ConcurrentDictionary<Guid, ConcurrentBag<MorpionPlayer>>();
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
            if (MorpionPlayerHandler.Players.Any(x => x.ClientId == Context.ConnectionId))
            {
                _logger.LogInformation("{player} disconnected", MorpionPlayerHandler.Players.FirstOrDefault(x => x.ClientId == Context.ConnectionId).Username);
                MorpionPlayerHandler.Players.RemoveWhere(x => x.ClientId == Context.ConnectionId);
            }
            return base.OnDisconnectedAsync(exception);
        }
        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.info, "Please register with /register username");
        }
        public async Task<bool> Register(string username)
        {
            if (!MorpionPlayerHandler.Players.Any(x => x.ClientId == Context.ConnectionId))
            {
                MorpionPlayerHandler.Players.Add(new MorpionPlayer(Context.ConnectionId, username));
                _logger.LogInformation("New challenger come in the ring : {0}", username);
                await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.info, $"Registered as {username}");
                return true;
            }
            else
            {
                _logger.LogError("Registration failed, already exist {0}", MorpionPlayerHandler.Players.FirstOrDefault(x => x.ClientId == Context.ConnectionId).Username);
                await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.error
                    , $"You are already registered as {MorpionPlayerHandler.Players.FirstOrDefault(x => x.ClientId == Context.ConnectionId).Username}");
                return false;
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
                var gameId = _morpionManager.JoinAGame(player);
                if (gameId.Equals(Guid.Empty))
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
                    await Clients.Client(players[0].ClientId).SendAsync(MorpionMessageHelper.playerToken, MorpionHelper.PLAYER1TOKEN);
                    await Clients.Client(players[1].ClientId).SendAsync(MorpionMessageHelper.playerToken, MorpionHelper.PLAYER2TOKEN);
                    await Clients.Clients(players.Select(x => x.ClientId)).SendAsync(MorpionMessageHelper.grille, _morpionManager.GetGrille(gameId));
                    await Clients.Client(players[0].ClientId).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.Play);
                    await Clients.Client(player.ClientId).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.WaitOpponent);
                    _logger.LogInformation("A Game begin, gameId : {gameId} with Player1 : {player1} and Player2: {player2}", gameId, players[0].Username, players[1].Username);
                }

            }
        }

        public async Task<bool> Turn(Guid gameId, int x, int y)
        {
            var players = _morpionManager.GetMorpionPlayers(gameId);
            var player = players.FirstOrDefault(x => x.ClientId == Context.ConnectionId);
            var opponent = players.FirstOrDefault(x => x != player);
            var value = false;
            if (_morpionManager.PlayerTurn(gameId, player, x, y))
            {
                await Clients.Clients(players.Select(x => x.ClientId)).SendAsync(MorpionMessageHelper.grille, _morpionManager.GetGrille(gameId));
                if (MorpionPlayerHandler.SpectateGames.TryGetValue(gameId, out var spectators))
                {
                    await Clients.Clients(spectators.Select(x => x.ClientId)).SendAsync(MorpionMessageHelper.grille, _morpionManager.GetGrille(gameId));
                }
                if (!_morpionManager.IsGameOver(gameId))
                {
                    await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.WaitOpponent);
                    await Clients.Client(opponent.ClientId).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.Play);
                }
                else
                {
                    await Clients.Clients(players.Select(x => x.ClientId)).SendAsync(MorpionMessageHelper.gameState, MorpionHelper.GameState.End);
                    await Clients.Clients(players.Select(x => x.ClientId)).SendAsync(MorpionMessageHelper.winner, _morpionManager.WinnerOfTheGame(gameId) is null ? "Draw" : _morpionManager.WinnerOfTheGame(gameId).Username);
                    if (spectators.Any())
                    {
                        await Clients.Clients(spectators.Select(x => x.ClientId)).SendAsync(MorpionMessageHelper.winner, _morpionManager.WinnerOfTheGame(gameId) is null ? "Draw" : _morpionManager.WinnerOfTheGame(gameId).Username);
                        MorpionPlayerHandler.SpectateGames.TryRemove(gameId, out var spect);
                    }
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

        public async Task Chat(string message)
        {
            if (MorpionPlayerHandler.Players.Any(x => x.ClientId == Context.ConnectionId))
            {
                var player = MorpionPlayerHandler.Players.First(x => x.ClientId == Context.ConnectionId);
                await Clients.AllExcept(player.ClientId).SendAsync(MorpionMessageHelper.chat, player.Username, message);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.error, $"You must register before with /register username");
            }
        }

        public Dictionary<Guid,List<MorpionPlayer>> Games()
        {
            return _morpionManager.GetAllGames();
        }

        public async Task<bool> Spectate(Guid gameId)
        {
            var spectator = MorpionPlayerHandler.Players.First(x => x.ClientId == Context.ConnectionId);
            if(!MorpionPlayerHandler.SpectateGames.ContainsKey(gameId))
            {
                var spectators = new ConcurrentBag<MorpionPlayer>();
                spectators.Add(spectator);
                MorpionPlayerHandler.SpectateGames.TryAdd(gameId, spectators);
                return true;
            }
            else
            {
                if(MorpionPlayerHandler.SpectateGames.TryGetValue(gameId, out var spectatedGame))
                {
                    spectatedGame.Add(spectator);
                    await Clients.Client(Context.ConnectionId).SendAsync(MorpionMessageHelper.grille, _morpionManager.GetGrille(gameId));
                }
                return false;
            }
        }
    }
}
