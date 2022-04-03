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
        private char _player = 'X';
        private char[,] _grille = new char[3,3];

        public Board()
        {

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
        }

        public void OnInput(InputEvent inputEvent)
        {
            var boardCell = (BoardCell)_board.GetChild(_i, _j);
            boardCell.UpdateCell(_grille[_i,_j], new Color(139, 69, 19).Mix(Color.White, ((_i + _j) % 2) == 1 ? 0f : 0.4f));

            if (!_keys.Contains(inputEvent.Key.Key)) return;
            if(inputEvent.Key.Key == ConsoleKey.LeftArrow)
            {
                if (_i > 0)
                {
                    _i--;
                }
            }
            if(inputEvent.Key.Key == ConsoleKey.RightArrow)
            {
                if (_i < 2)
                {
                    _i++;
                }
            }
            if(inputEvent.Key.Key == ConsoleKey.UpArrow)
            {
                if (_j > 0)
                {
                    _j--;
                }

            }
            if(inputEvent.Key.Key == ConsoleKey.DownArrow)
            {
                if(_j < 2)
                {
                    _j++;
                }
            }
            if(inputEvent.Key.Key == ConsoleKey.Tab)
            {
                _grille[_i, _j] = _player;
            }
            boardCell = (BoardCell)_board.GetChild(_i, _j);
            boardCell.UpdateCell(_player, new Color(139, 69, 19).Mix(Color.White, ((_i + _j) % 2) == 1 ? 0f : 0.4f));

            inputEvent.Handled = true;
        }
    }
}
