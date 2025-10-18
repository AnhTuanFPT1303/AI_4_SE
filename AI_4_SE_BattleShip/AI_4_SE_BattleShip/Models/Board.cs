namespace AI_4_SE_BattleShip.Models
{
    public class Board
    {
        // 2D integer grid representing the board (rows x columns). Default 7x7.
        public int[,] Size { get; set; } = new int[7, 7];

        public List<Ship> Ships { get; set; } = new List<Ship>();
        public List<Coordinate> ShotsFired { get; set; } = new List<Coordinate>();

        public bool IsAllShipsSunk => Ships != null && Ships.All(s => s.IsSunk);
    }
}
