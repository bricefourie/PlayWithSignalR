using ConsoleGUI.Controls;
using ConsoleGUI.UserDefined;
using ConsoleGUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorpionClientV2
{
    public class LogPanel : SimpleControl
    {
        private readonly VerticalStackPanel _verticalStackPanel;
        public LogPanel()
        {
            _verticalStackPanel = new VerticalStackPanel();
            Content = _verticalStackPanel;
        }

        public void Add(string message,string username)
        {
            _verticalStackPanel.Add(new WrapPanel
            {
                Content = new HorizontalStackPanel
                {
                    Children = new[]
                    {
                        new TextBlock {Text = $"[{DateTime.Now.ToLongTimeString()}]", Color = new Color(200,20,20)},
                        new TextBlock {Text = $"[{username}]", Color = new Color(56,52,52)},
                        new TextBlock {Text = message}
                    }
                }
            });
        }
    }
}
