using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using static MorpionClient.Enums;

namespace MorpionClient
{
    class Program
    {
        static Guid GameId;
        static string PlayerToken;
        static HubConnection connection;
        static bool IsGameOver = false;
        static async Task Main(string[] args)
        {
            
                connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/Morpion")
                .AddNewtonsoftJsonProtocol()
                .Build();
            
            connection.On<string[,]>(Message.grille.ToString(),
                (grille)
                =>
                {
                    PrintGrille(grille);
                });
            connection.On<int>(Message.gameState.ToString(), (state) =>
             {
                 switch (state)
                 {
                     case 0:
                         Console.WriteLine("Waiting for an opponent...");
                         break;
                     case 1:
                         Console.WriteLine("Opponent is playing...");
                         break;
                     case 2:
                         Console.WriteLine("It's your turn !");
                         Play();
                         break;
                     case 3:
                         Console.WriteLine("GameOver!");
                         break;
                     default:
                         break;
                 }
             });
            connection.On<Guid>(Message.gameId.ToString(), (gameId) =>
             {
                 GameId = gameId;
             });
            connection.On<string>(Message.playerToken.ToString(), (playerToken) =>
            {
                PlayerToken = playerToken;
            });
            connection.On<string>(Message.winner.ToString(), (winner) =>
             {
                 if(string.IsNullOrWhiteSpace(winner))
                 {
                     Console.WriteLine("This is a Draw!");
                 }
                 else
                 {
                     Console.WriteLine($"Congratulation, the winner is {winner}");
                 }
                 IsGameOver = true;
             });
            await connection.StartAsync();
            Console.WriteLine("Username :");
            var username = Console.ReadLine();
            await connection.SendAsync(Methods.Register.ToString(),username);
            await connection.SendAsync(Methods.Join.ToString());
            while (!IsGameOver)
            {

            }
            Console.WriteLine("End of game");
            Console.Read();
        }

        static void PrintGrille(string[,] grille)
        {
            Console.Clear();
            for (int i = 0; i < grille.GetLength(0); i++)
            {
                for (int j = 0; j < grille.GetLength(1); j++)
                {
                    if (string.IsNullOrWhiteSpace(grille[i, j]))
                    {
                        Console.Write(" ");
                    }
                    else
                    {
                        Console.Write(grille[i, j]);
                    }
                    if(j < grille.GetLength(1)-1)
                    {
                        Console.Write(" | ");
                    }
                }
                Console.WriteLine();
            }
        }
        static string[,] GetRandomGrille()
        {
            var grille = new string[3, 3];
            for (int i = 0; i < grille.GetLength(0); i++)
            {
                for (int j = 0; j < grille.GetLength(1); j++)
                {
                    var random = new Random().Next(0, 3);
                    switch (random)
                    {
                        case 0:
                            grille[i, j] = string.Empty;
                            break;
                        case 1:
                            grille[i, j] = "X";
                            break;
                        case 2:
                            grille[i, j] = "O";
                            break;
                        default:
                            break;
                    }
                }
            }
            return grille;
        }
        static void Play()
        {
            Console.WriteLine("x : ");
            int x = int.Parse(Console.ReadLine());
            Console.WriteLine("y : ");
            int y = int.Parse(Console.ReadLine());
            connection.SendAsync(Methods.Turn.ToString(), GameId, x, y);
        }
    }
}
