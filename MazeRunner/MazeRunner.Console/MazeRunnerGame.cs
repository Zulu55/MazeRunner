using MazeRunner.Models.Enunms;
using MazeRunner.Models.Requests;
using MazeRunner.Models.Responses;
using MazeRunner.Services;

namespace MazeRunner.Console
{
    public class MazeRunnerGame
    {
        private readonly IApiService _apiService;
        private Guid _mazeUid;
        private Guid _gameUid;
        private bool _completed;

        public MazeRunnerGame(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<string> CreateMazeAsync()
        {
            var request = new CreateNewMazeRequest
            {
                Height = 25,
                Width = 25
            };
            var response = await _apiService.PostAsync<CreateNewMazeRequest, CreateNewMazeResponse>("/api", "/Maze", request);
            if (!response.WasSuccess)
            {
                return "The maze could not be created.";
            }

            _mazeUid = response.Result!.MazeUid;
            return $"Maze {_mazeUid} created.";
        }

        public async Task<string> CreateGameAsync()
        {
            var request = new OperationRequest
            {
                Operation = Operations.Start.ToString(),
            };
            var response = await _apiService.PostAsync<OperationRequest, CreateNewGameResponse>("/api", $"/Game/{_mazeUid}", request);
            if (!response.WasSuccess)
            {
                return "The game could not be created.";
            }

            _gameUid = response.Result!.GameUid;
            return $"Game {_gameUid} created.";
        }

        public async Task<string> RunGameAsync()
        {
            var movement = Operations.GoEast;
            var movements = "Movements: ";
            var movementsCount = 0;
            while (!_completed || movementsCount > 625)
            {
                var request = new OperationRequest
                {
                    Operation = movement.ToString(),
                };
                movementsCount++;
                var response = await _apiService.PostAsync<OperationRequest, TakeALookResponse>("/api", $"/Game/{_mazeUid}/{_gameUid}/", request);
                if (!response.WasSuccess)
                {
                    return $"{movements}\nError trying the next movement.";
                }

                movements += $"{movement} ({response.Result!.MazeBlockView.CoordX}, {response.Result!.MazeBlockView.CoordY})\n ";
                _completed = response.Result!.Game.Completed;
                if (_completed)
                {
                    break;
                }

                if (!response.Result.MazeBlockView.SouthBlocked)
                {
                        movement = Operations.GoSouth;
                }
                else if (!response.Result.MazeBlockView.EastBlocked)
                {
                        movement = Operations.GoEast;
                }
                else if (!response.Result.MazeBlockView.WestBlocked)
                { 
                        movement = Operations.GoWest;
                }
                else
                {
                        movement = Operations.GoNorth;
                }
            }

            if (_completed)
            {
                return $"{movements}\nGame over.";
            }

            return "The maze has no solution\nGame over.";
        }
    }
}