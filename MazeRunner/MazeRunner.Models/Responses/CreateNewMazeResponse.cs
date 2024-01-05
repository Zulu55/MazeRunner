namespace MazeRunner.Models.Responses
{
    public class CreateNewMazeResponse
    {
        public Guid MazeUid { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}