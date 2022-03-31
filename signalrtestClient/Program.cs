using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
namespace signalrtestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Username :");
            var username = Console.ReadLine();
            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/Chathub")
                .Build();
            connection.On<string, string>("newmsg",
                (sender, message)
                =>
                {
                    Console.WriteLine($"{sender}:{message}");
                });
            await connection.StartAsync();
            while (true)
            {
                var message = Console.ReadLine();
                await connection.SendAsync("SendMessage", username, message);
            }
        }
    }
}
