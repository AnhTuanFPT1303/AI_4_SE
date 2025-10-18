namespace AI_4_SE_BattleShip.Models
{
    public class Board
    {
        public int Size { get; set; } = 7;
        public List<Ship> Ships { get; set; }
        public List<Coordinate> ShotsFired { get; set; }

        public bool IsAllShipsSunk => Ships.All(s => s.IsSunk);
    }

}
