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
            var responseWhereIAm = await _apiService.GetAsync<TakeALookResponse>("/api", $"/Game/{_mazeUid}/{_gameUid}/");
            if (!responseWhereIAm.WasSuccess)
            {
                return "Error getting the current position.";
            }

            var movement = Operations.GoEast;
            _direction = Operations.GoEast;
            var previousBlock = responseWhereIAm.Result!.MazeBlockView;

            if (previousBlock.EastBlocked)
            {
                movement = Operations.GoSouth;
                _direction = Operations.GoSouth;
            }

            var movements = "Movements:\n";
            var movementsCount = 0;
            var maxMovements = _mazeSize * _mazeSize * 2;
            while (!_completed && movementsCount < maxMovements)
            {
                var request = new OperationRequest
                {
                    Operation = movement.ToString(),
                };

                var responseMovement = await _apiService.PostAsync<OperationRequest, TakeALookResponse>("/api", $"/Game/{_mazeUid}/{_gameUid}/", request);
                movementsCount++;
                int count = 0;
                do
                {
                    if (!responseMovement.WasSuccess)
                    {
                        count++;
                        if (!previousBlock.EastBlocked)
                        {
                            movement = Operations.GoEast;
                            _direction = Operations.GoEast;
                        }
                        else if (!previousBlock.SouthBlocked)
                        {
                            movement = Operations.GoSouth;
                            _direction = Operations.GoSouth;
                        }
                        else if (!previousBlock.WestBlocked)
                        {
                            movement = Operations.GoWest;
                            _direction = Operations.GoWest;
                        }
                        else if (!previousBlock.NorthBlocked)
                        {
                            movement = Operations.GoNorth;
                            _direction = Operations.GoNorth;
                        }

                        count++;
                        movementsCount++;
                        request = new OperationRequest
                        {
                            Operation = movement.ToString(),
                        };
                        responseMovement = await _apiService.PostAsync<OperationRequest, TakeALookResponse>("/api", $"/Game/{_mazeUid}/{_gameUid}/", request);
                    }
                } while (!responseMovement.WasSuccess && count < 4);

                if (count > 3)
                {
                    return $"{movements}\nThe maze has no solution\nGame over.";
                }

                movements += $"{movement} ({responseMovement.Result!.MazeBlockView.CoordX}, {responseMovement.Result!.MazeBlockView.CoordY})\n ";
                //TODO: Uncomment just to debug purposes
                //var lastMovement = movements.Length > 80 ? movements.Substring(movements.Length - 80, 80) : movements;
                _completed = responseMovement.Result!.Game.Completed;
                if (_completed)
                {
                    break;
                }

                previousBlock = responseMovement.Result.MazeBlockView;
                movement = GetNextMovement(responseMovement.Result.MazeBlockView);
            }

            if (_completed)
            {
                return $"{movements}\nGame over.";
            }

            return $"{movements}\nThe maze has no solution\nGame over.";
        }

        private Operations GetNextMovement(Mazeblockview mazeBlockView)
        {
            switch (_direction)
            {
                case Operations.GoEast:
                    if (!mazeBlockView.EastBlocked) return Operations.GoEast;
                    if (!mazeBlockView.SouthBlocked) return Operations.GoSouth;
                    _direction = Operations.GoWest;
                    return Operations.GoWest;

                case Operations.GoWest:
                    if (!mazeBlockView.SouthBlocked)
                    {
                        _direction = Operations.GoSouth;
                        return Operations.GoSouth;
                    }
                    if (!mazeBlockView.WestBlocked) return Operations.GoWest;
                    _direction = Operations.GoNorth;
                    return Operations.GoNorth;

                case Operations.GoSouth:
                    if (!mazeBlockView.SouthBlocked) return Operations.GoSouth;
                    if (!mazeBlockView.EastBlocked) return Operations.GoEast;
                    _direction = Operations.GoNorth;
                    return Operations.GoNorth;

                case Operations.GoNorth:
                    if (!mazeBlockView.EastBlocked)
                    {
                        _direction = Operations.GoEast;
                        return Operations.GoEast;
                    }
                    return Operations.GoNorth;
            }
            return Operations.GoEast;
        }

        public async Task<int[,]?> GetMazeAsync()
        {
            var response = await _apiService.GetAsync<MazeResponse>("/api", $"/Maze/{_mazeUid}/");
            if (!response.WasSuccess)
            {
                return null;
            }
            var maze = new int[_mazeSize + 2, _mazeSize + 2];
            foreach (var block in response.Result!.Blocks)
            {
                block.CoordX++;
                block.CoordY++;
                if (block.NorthBlocked) maze[block.CoordX - 1, block.CoordY] = 1;
                if (block.WestBlocked) maze[block.CoordX, block.CoordY - 1] = 1;
                if (block.EastBlocked) maze[block.CoordX, block.CoordY + 1] = 1;
                if (block.SouthBlocked) maze[block.CoordX + 1, block.CoordY] = 1;
            }
            maze[0, 0] = 1;
            return maze;
        }
    }
}