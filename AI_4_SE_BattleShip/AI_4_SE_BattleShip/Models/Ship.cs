namespace AI_4_SE_BattleShip.Models
{
    public class Ship
    {
        public string Name { get; set; } // Ship1, Ship2, Ship3
        public int Length { get; set; } // Số ô chiếm
        public List<Coordinate> Positions { get; set; } // Vị trí trên bản đồ
        public bool IsSunk => Positions.All(p => p.IsHit);
    }

}
