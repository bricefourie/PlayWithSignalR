using System;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;
using ConsoleGUI;
using ConsoleGUI.UserDefined;
using System.Threading;
using ConsoleGUI.Api;
using MorpionClientV2.Managers;
using System.Threading.Tasks;

namespace MorpionClientV2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var morpionManager = new MorpionManager();
            var textBox = new TextBox();
            var mainConsole = new LogPanel();
            var tabPanel = new TabPanel();
            var board = new Board();
            tabPanel.AddTab("Game", new Box()
            {
                HorizontalContentPlacement = Box.HorizontalPlacement.Center,
                VerticalContentPlacement = Box.VerticalPlacement.Center,
                Content = new Board()
            });
            var dockPanel = new DockPanel
            {
                Placement = DockPanel.DockedControlPlacement.Top,
                FillingControl = new Overlay
                {
                    BottomContent = new Background
                    {
                        Color = new Color(25, 54, 65),
                        Content = new DockPanel
                        {
                            Placement = DockPanel.DockedControlPlacement.Right,
                            DockedControl = new Background
                            {
                                Color = new Color(30, 40, 50),
                                Content = new Border
                                {
                                    BorderPlacement = BorderPlacement.Left,
                                    BorderStyle = BorderStyle.Double.WithColor(new Color(50, 60, 70)),
                                    Content = new Boundary
                                    {
                                        MinWidth = 50,
                                        MaxWidth = 50,
                                        Content = new DockPanel
                                        {
                                            Placement = DockPanel.DockedControlPlacement.Bottom,
                                            DockedControl = new Boundary
                                            {
                                                MaxHeight = 1,
                                                Content = new HorizontalStackPanel
                                                {
                                                    Children = new IControl[]
                                                    {
                                                        new Style
                                                        {
                                                            Foreground = new Color(150, 150, 200),
                                                            Content = new TextBlock { Text = @">" }
                                                        },
                                                        textBox
                                                    }
                                                }
                                            },
                                            FillingControl = new Box
                                            {
                                                VerticalContentPlacement = Box.VerticalPlacement.Bottom,
                                                HorizontalContentPlacement = Box.HorizontalPlacement.Stretch,
                                                Content = mainConsole
                                            }
                                        }
                                    }
                                }
                            },
                            FillingControl = new Box
                            {
                                VerticalContentPlacement = Box.VerticalPlacement.Center,
                                HorizontalContentPlacement = Box.HorizontalPlacement.Center,
                                Content = board
                            }
                        }
                    }
                }
            };

            var input = new IInputListener[]
            {
                board,
                new InputController(textBox,mainConsole,"Anonymous"),
                textBox
            };

            await morpionManager.HubConnection.StartAsync();
            ConsoleManager.Setup();
            ConsoleManager.Resize(new Size(150, 40));
            ConsoleManager.Content = dockPanel;
            for (int i = 0; ; i++)
            {
                Thread.Sleep(10);
                ConsoleManager.ReadInput(input);
                ConsoleManager.AdjustBufferSize();
                ConsoleManager.Content.Context.Redraw(dockPanel);
            }

        }

    }
}
