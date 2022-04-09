using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalrtest.Morpion
{
    public interface IMorpionManager
    {
        Guid GameIdOfPlayer(MorpionPlayer Player);
        Guid JoinAGame(MorpionPlayer Player);
        Guid NewMorpionGame(MorpionPlayer Player);
        bool PlayerTurn(Guid GameId, MorpionPlayer Player, int x, int y);
        MorpionPlayer WinnerOfTheGame(Guid GameId);
        MorpionPlayer[] GetMorpionPlayers(Guid GameId);
        bool IsGameOver(Guid GameId);
        string[,] GetGrille(Guid GameId);
        bool DeleteGame(Guid GameId);
        Dictionary<Guid, List<MorpionPlayer>> GetAllGames();

    }
    public class MorpionManager : IMorpionManager
    {
        private ConcurrentDictionary<Guid, MorpionGame> _morpionGames;
        public MorpionManager()
        {
            _morpionGames = new ConcurrentDictionary<Guid, MorpionGame>();
        }

        public Guid JoinAGame(MorpionPlayer Player)
        {
            var availableGame = _morpionGames.FirstOrDefault(x => x.Value.Player2 is null);
            if (!availableGame.Equals(default(KeyValuePair<Guid, MorpionGame>)))
            {
                availableGame.Value.Player2 = Player;
                return availableGame.Key;
            }
            return Guid.Empty;
        }

        public Guid NewMorpionGame(MorpionPlayer Player)
        {
            Guid gameId = Guid.NewGuid();
            if (_morpionGames.TryAdd(gameId, new MorpionGame() { Player1 = Player, Morpion = new Morpion() }))
            {
                return gameId;
            }
            return Guid.Empty;
        }

        public Guid GameIdOfPlayer(MorpionPlayer Player)
        {
            var searchGame = _morpionGames.Where(x => x.Value.Player1.ClientId == Player.ClientId || x.Value.Player2.ClientId == Player.ClientId)
                .Where(x => !x.Value.Morpion.IsGameOver());
            if (searchGame.Any())
            {
                return searchGame.FirstOrDefault().Key;
            }
            return Guid.Empty;
        }

        public bool PlayerTurn(Guid GameId, MorpionPlayer Player, int x, int y)
        {
            if (_morpionGames.TryGetValue(GameId, out MorpionGame value))
            {

                return value.Morpion.SetToken(x, y, GetPlayerIndex(value, Player));
            }
            return false;
        }

        private int GetPlayerIndex(MorpionGame morpionGame, MorpionPlayer player)
        {
            if (morpionGame.Player1.ClientId == player.ClientId)
            {
                return 1;
            }
            if (morpionGame.Player2.ClientId == player.ClientId)
            {
                return 2;
            }
            return 0;
        }

        public MorpionPlayer WinnerOfTheGame(Guid GameId)
        {
            if (_morpionGames.TryGetValue(GameId, out MorpionGame value))
            {
                switch (value.Morpion.Winner())
                {
                    case 1:
                        return value.Player1;
                    case 2:
                        return value.Player2;
                    default:
                        return null;
                }
            }
            return null;
        }

        public MorpionPlayer[] GetMorpionPlayers(Guid GameId)
        {
            if (_morpionGames.TryGetValue(GameId, out MorpionGame game))
            {
                var players = new MorpionPlayer[2];
                players[0] = game.Player1;
                players[1] = game.Player2;
                return players;
            }
            return null;
        }

        public bool IsGameOver(Guid GameId)
        {
            if (_morpionGames.TryGetValue(GameId, out MorpionGame game))
            {
                return game.Morpion.IsGameOver();
            }
            return false;
        }

        public string[,] GetGrille(Guid GameId)
        {
            if (_morpionGames.TryGetValue(GameId, out MorpionGame game))
            {
                return game.Morpion.grille;
            }
            return new string[3, 3];
        }

        public bool DeleteGame(Guid GameId)
        {
            return _morpionGames.TryRemove(GameId, out var morpionGame);
        }

        public Dictionary<Guid, List<MorpionPlayer>> GetAllGames()
        {
            Dictionary<Guid, List<MorpionPlayer>> result = new Dictionary<Guid, List<MorpionPlayer>>();
            foreach (var game in _morpionGames)
            {
                result.Add(game.Key, new List<MorpionPlayer> { game.Value.Player1, game.Value.Player2 });
            }
            return result;
        }
    }
    class MorpionGame
    {
        public MorpionPlayer Player1 { get; set; }
        public MorpionPlayer Player2 { get; set; }
        public Morpion Morpion { get; set; }
    }
}
