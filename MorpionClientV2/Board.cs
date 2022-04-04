using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleGUI;
using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;
using ConsoleGUI.UserDefined;
using MorpionClientV2.Managers;
using Microsoft.AspNetCore.SignalR.Client;
using MorpionClientV2.Helpers;
using MorpionClientV2.Enums;

namespace MorpionClientV2
{

    internal class BoardCell : SimpleControl
    {
        private readonly IControl _cell;
        public BoardCell(char content, Color color)
        {
            _cell = new Background
            {
                Color = color,
                Content = new Box
                {
                    HorizontalContentPlacement = Box.HorizontalPlacement.Center,
                    VerticalContentPlacement = Box.VerticalPlacement.Center,
                    Content = new TextBlock { Text = content.ToString() }
                }
            };
            Content = _cell;
        }
        public void UpdateCell(char content, Color color)
        {
            Content = new BoardCell(content, color);
        }
    }
    public class Board : SimpleControl, IInputListener
    {
        private readonly Grid _board;
        private static List<ConsoleKey> _keys = new List<ConsoleKey>() { ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow, ConsoleKey.Tab };
        private int _i = 0;
        private int _j = 0;
        private char _player = ' ';
        private char[,] _grille = new char[3,3];
        private readonly MorpionManager _morpionManager;
        private Guid _gameId = Guid.Empty;
        private int _gameState = 0;

        public Guid GameId
        {
            get { return _gameId; }
            set { _gameId = value; }
        }

        public Board(MorpionManager morpionManager)
        {
            _morpionManager = morpionManager;
            _board = new Grid
            {
                Rows = Enumerable.Repeat(new Grid.RowDefinition(5), 3).ToArray(),
                Columns = Enumerable.Repeat(new Grid.ColumnDefinition(10), 3).ToArray(),
            };
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _board.AddChild(i, j, new BoardCell(' ', new Color(139, 69, 19).Mix(Color.White, ((i + j) % 2) == 1 ? 0f : 0.4f)));
                    _grille[i, j] = ' ';
                }
            }
            Content = _board;
            _morpionManager.HubConnection.On<string>(MorpionMessageHelper.playerToken, (playerToken) => _player = playerToken.ToCharArray()[0]);
            _morpionManager.HubConnection.On<Guid>(MorpionMessageHelper.gameId, (gameId) => _gameId = gameId);
            _morpionManager.HubConnection.On<int>(MorpionMessageHelper.gameState, (gameState) => _gameState = gameState);
            _morpionManager.HubConnection.On<string[,]>(MorpionMessageHelper.grille, (grille) => { StringArrayToCharArray(grille) ;Redraw(); });

        }

        //To prevent to break server logic. If I wasn't lazy, I will change it on server side.
        private void StringArrayToCharArray(string[,] grille)
        {
            for (int i = 0; i < grille.GetLength(0); i++)
            {
                for (int j = 0; j < grille.GetLength(1); j++)
                {
                    // Yes, it's not very nice.
                    if (string.IsNullOrWhiteSpace(grille[i, j]))
                    {
                        _grille[i, j] = ' ';
                    }
                    else
                    {
                        _grille[i, j] = grille[i, j].ToCharArray()[0];
                    }
                }
            }
        }

        private void Redraw()
        {
            for (int i = 0; i < _grille.GetLength(0); i++)
            {
                for (int j = 0; j < _grille.GetLength(1); j++)
                {
                    var boardCell = (BoardCell)_board.GetChild(i, j);
                    boardCell.UpdateCell(_grille[i,j], new Color(139, 69, 19).Mix(Color.White, ((i + j) % 2) == 1 ? 0f : 0.4f));
                }
            }
        }

        public async void OnInput(InputEvent inputEvent)
        {
            if (!_keys.Contains(inputEvent.Key.Key)) return;
            if (_gameId != Guid.Empty && _gameState == ((int)GameState.Play))
            {
                var boardCell = (BoardCell)_board.GetChild(_i, _j);
                boardCell.UpdateCell(_grille[_i, _j], new Color(139, 69, 19).Mix(Color.White, ((_i + _j) % 2) == 1 ? 0f : 0.4f));
                if (inputEvent.Key.Key == ConsoleKey.LeftArrow)
                {
                    if (_i > 0)
                    {
                        _i--;
                    }
                }
                if (inputEvent.Key.Key == ConsoleKey.RightArrow)
                {
                    if (_i < 2)
                    {
                        _i++;
                    }
                }
                if (inputEvent.Key.Key == ConsoleKey.UpArrow)
                {
                    if (_j > 0)
                    {
                        _j--;
                    }

                }
                if (inputEvent.Key.Key == ConsoleKey.DownArrow)
                {
                    if (_j < 2)
                    {
                        _j++;
                    }
                }
                if (inputEvent.Key.Key == ConsoleKey.Tab)
                {
                    if (await _morpionManager.Turn(_gameId, _i, _j))
                    {
                        _grille[_i, _j] = _player;
                    }
                }
                boardCell = (BoardCell)_board.GetChild(_i, _j);
                boardCell.UpdateCell(_player, new Color(139, 69, 19).Mix(Color.White, ((_i + _j) % 2) == 1 ? 0f : 0.4f));
            }
            inputEvent.Handled = true;
        }
    }
}
