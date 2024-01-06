using MazeRunner.Models.Models;

namespace MazeRunner.Models.Responses
{
    public class MazeResponse
    {
        public Guid MazeUid { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Block> Blocks { get; set; } = null!;
    }
}