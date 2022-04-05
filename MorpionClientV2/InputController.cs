using ConsoleGUI.Controls;
using ConsoleGUI.Input;
using Microsoft.AspNetCore.SignalR.Client;
using MorpionClientV2.Helpers;
using MorpionClientV2.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorpionClientV2
{
    public class InputController : IInputListener
    {
        private readonly TextBox _textBox;
        private readonly LogPanel _logPanel;
        private string _username;
        private MorpionManager _morpionManager;

        public InputController(TextBox textBox, LogPanel logPanel, string username, MorpionManager morpionManager)
        {
            _textBox = textBox;
            _logPanel = logPanel;
            _username = username;
            _morpionManager = morpionManager;
        }

        public async void OnInput(InputEvent inputEvent)
        {
            if (inputEvent.Key.Key != ConsoleKey.Enter) return;
            _logPanel.Add(_textBox.Text, _username);
            if (_textBox.Text.StartsWith("/"))
            {
                string[] command = _textBox.Text.Trim('/').Split(' ');
                await CommandManager(command[0], command.Skip(1).ToArray());
            }
            else
            {
                await _morpionManager.Chat(_textBox.Text);
            }
            _textBox.Text = string.Empty;
            inputEvent.Handled = true;
        }

        private async Task CommandManager(string command, string[] args = null)
        {
            switch (command)
            {
                case CommandHelper.Help:
                    _logPanel.Add($"/{CommandHelper.Register} : register your username to the server", "Console");
                    _logPanel.Add($"/{CommandHelper.Join} : join or create a game", "Console");
                    _logPanel.Add($"/{CommandHelper.Help} : show this help", "Console");
                    _logPanel.Add("In game, use Arrow to select your cell, and TAB to send your choice", "Console");
                    break;
                case CommandHelper.Register:
                    if (args != null && args.Any())
                    {
                        _logPanel.Add($"Send register as {args[0]}", "Console");
                        if(await _morpionManager.Register(args[0]))
                        {
                            _username = args[0];
                        }

                    }
                    break;
                case CommandHelper.Join:
                    await _morpionManager.Join();
                    break;
                case CommandHelper.Clear:
                    _logPanel.Clear();
                    break;
                default:
                    break;
            }
        }
    }
}
