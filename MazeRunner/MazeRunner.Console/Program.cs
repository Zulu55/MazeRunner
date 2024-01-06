using MazeRunner.Console;
using MazeRunner.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IApiService, ApiService>();
            })
            .Build();

        Console.Clear();
        Console.WriteLine("Begin.");
        var apiService = host.Services.GetService<IApiService>();
        var game = new MazeRunnerGame(apiService!);
        Console.WriteLine(await game.CreateMazeAsync());
        Console.WriteLine(await game.CreateGameAsync());
        Console.WriteLine("Finding solution, please wait, this process could take a long time...");
        //TODO: Uncomment just to debug purposes
        //var maze = await game.GetMazeAsync();
        //if (maze != null)
        //{
        //    ShowMaze(maze);
        //}
        Console.WriteLine(await game.RunGameAsync());
        Console.WriteLine("End.");
    }

    private static void ShowMaze(int[,]? maze)
    {
        for (int i = 0; i < maze!.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                Console.Write($"{maze[i, j]} ");
            }
            Console.WriteLine();
        }
    }
}