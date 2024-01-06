namespace MazeRunner.Models.Models
{
    public class Block
    {
        public int CoordX { get; set; }
        public int CoordY { get; set; }
        public bool NorthBlocked { get; set; }
        public bool SouthBlocked { get; set; }
        public bool WestBlocked { get; set; }
        public bool EastBlocked { get; set; }
    }
}