using MazeRunner.Models.Requests;
using MazeRunner.Models.Responses;
using MazeRunner.Services;

namespace MazeRunner.Console
{
    public class MazeRunnerGame
    {
        private readonly IApiService _apiService;

        public MazeRunnerGame(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<string> StartAsync()
        {
            var request = new CreateNewMazeRequest
            {
                Height = 25,
                Width = 25
            };
            var response = await _apiService.PostAsync<CreateNewMazeRequest, CreateNewMazeResponse>("/api", "/Maze", request);
            if (!response.WasSuccess)
            {
                return "The maze could not be created";
            }

            return "End";
        }
    }
}