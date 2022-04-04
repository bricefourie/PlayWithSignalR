using ConsoleGUI.Controls;
using ConsoleGUI.Input;
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

        public InputController(TextBox textBox, LogPanel logPanel,string username)
        {
            _textBox = textBox;
            _logPanel = logPanel;
            _username = username;
        }

        public void ChangeUsername(string username)
        {
            _username = username;
        }

        public void OnInput(InputEvent inputEvent)
        {
            if (inputEvent.Key.Key != ConsoleKey.Enter) return;
            _logPanel.Add(_textBox.Text,_username);
            _textBox.Text = string.Empty;
            inputEvent.Handled = true;
        }
    }
}
