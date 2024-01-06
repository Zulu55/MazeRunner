using MazeRunner.Models.Enunms;
using MazeRunner.Models.Models;
using MazeRunner.Models.Requests;
using MazeRunner.Models.Responses;
using MazeRunner.Services;

namespace MazeRunner.Console
{
    public class MazeRunnerGame
    {
        private readonly IApiService _apiService;
        private readonly int _mazeSize;
        private Guid _mazeUid;
        private Guid _gameUid;
        private bool _completed;
        private Operations _direction;

        public MazeRunnerGame(IApiService apiService)
        {
            _apiService = apiService;
            _mazeSize = 10;
        }

        public async Task<string> CreateMazeAsync()
        {
            var request = new CreateNewMazeRequest
            {
                Height = _mazeSize,
                Width = _mazeSize
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
            _direction = Operations.GoEast;
            var movements = "Movements:\n";
            var movementsCount = 0;
            var maxMovements = _mazeSize * _mazeSize * 2;
            while (!_completed && movementsCount < maxMovements)
            {
                var request = new OperationRequest
                {
                    Operation = movement.ToString(),
                };
                movementsCount++;
                var response = await _apiService.PostAsync<OperationRequest, TakeALookResponse>("/api", $"/Game/{_mazeUid}/{_gameUid}/", request);
                if (!response.WasSuccess)
                {
                    if (_direction == Operations.GoEast) _direction = Operations.GoSouth;
                    else if (_direction == Operations.GoSouth) _direction = Operations.GoWest;
                    else if (_direction == Operations.GoWest) _direction = Operations.GoNorth;
                    else return $"{movements}\nError trying the next movement.";
                    continue;
                }

                movements += $"{movement} ({response.Result!.MazeBlockView.CoordX}, {response.Result!.MazeBlockView.CoordY})\n ";
                var lastMovement = movements.Length > 80 ? movements.Substring(movements.Length - 80, 80) : movements;
                _completed = response.Result!.Game.Completed;
                if (_completed)
                {
                    break;
                }

                movement = GetNextMovement(response.Result.MazeBlockView);
            }

            if (_completed)
            {
                return $"{movements}\nGame over.";
            }

            return $"{movements}\nThe maze has no solution\nGame over.";
        }

        private Operations GetNextMovement(Mazeblockview mazeBlockView, Operations _direction = Operations.GoEast)
        {
            switch (_direction)
            {
                case Operations.GoEast:
                    if (!mazeBlockView.EastBlocked) return Operations.GoEast;
                    if (!mazeBlockView.SouthBlocked) return Operations.GoSouth;
                    _direction = Operations.GoWest;
                    return _direction;

                case Operations.GoWest:
                    if (!mazeBlockView.SouthBlocked)
                    {
                        _direction = Operations.GoSouth;
                        return _direction;
                    }
                    if (!mazeBlockView.WestBlocked) return Operations.GoWest;
                    _direction = Operations.GoNorth;
                    return _direction;

                case Operations.GoSouth:
                    if (!mazeBlockView.SouthBlocked) return Operations.GoSouth;
                    if (!mazeBlockView.EastBlocked) return Operations.GoEast;
                    _direction = Operations.GoNorth;
                    return _direction;

                case Operations.GoNorth:
                    if (!mazeBlockView.EastBlocked)
                    {
                        _direction = Operations.GoEast;
                        return _direction;
                    }
                    return Operations.GoNorth;
            }
            return Operations.GoEast;
        }
    }
}