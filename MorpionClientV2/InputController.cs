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

        public void ChangeUsername(string username)
        {
            _username = username;
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
                    break;
                case CommandHelper.Register:
                    if (args != null && args.Any())
                    {
                        //Register here
                        _logPanel.Add($"Send register as {args[0]}", "Console");
                        await _morpionManager.Register(args[0]);
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
