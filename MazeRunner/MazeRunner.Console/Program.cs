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

        Console.WriteLine("Begin");
        var apiService = host.Services.GetService<IApiService>();
        var game = new MazeRunnerGame(apiService!);
        Console.WriteLine(await game.CreateMazeAsync());
        Console.WriteLine(await game.CreateGameAsync());
        Console.WriteLine(await game.RunGameAsync());
    }
}