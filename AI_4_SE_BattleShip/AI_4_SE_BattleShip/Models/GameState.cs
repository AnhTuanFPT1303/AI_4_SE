namespace AI_4_SE_BattleShip.Models
{
    public class GameState
    {
        public Player PlayerA { get; set; }
        public Player PlayerB { get; set; }
        public int TurnCount { get; set; }
        public Player CurrentPlayer { get; set; }
        public bool IsGameOver => PlayerA.Board.IsAllShipsSunk || PlayerB.Board.IsAllShipsSunk;
    }

}
