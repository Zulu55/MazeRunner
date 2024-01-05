using MazeRunner.Models.Models;

namespace MazeRunner.Models.Responses
{
    public class TakeALookResponse
    {
        public Game Game { get; set; } = null!;
        public Mazeblockview MazeBlockView { get; set; } = null!;
    }
}