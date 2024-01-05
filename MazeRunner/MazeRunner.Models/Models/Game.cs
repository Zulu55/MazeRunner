namespace MazeRunner.Models.Models
{
    public class Game
    {
        public Guid MazeUid { get; set; }
        public Guid GameUid { get; set; }
        public bool Completed { get; set; }
        public int CurrentPositionX { get; set; }
        public int CurrentPositionY { get; set; }
    }
}