using Microsoft.AspNetCore.Mvc;
using AI_4_SE_BattleShip.Models;

namespace AI_4_SE_BattleShip.Controllers
{
    public class GameStateController : Controller
    {
        private static GameState _state;
        private static readonly int Rows = 7;
        private static readonly int Cols = 7;
        private static readonly Random _rand = new();
        private static readonly int[] ShipLengths = new[] { 3, 4, 2 };

        public IActionResult Index()
        {
            if (_state == null)
                NewGameInternal(false);

            return View(_state);
        }

        [HttpPost]
        public IActionResult NewGame(bool hotseat = false)
        {
            NewGameInternal(hotseat);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Skip()
        {
            if (_state == null)
                NewGameInternal(false);

            // Skip current player's turn: increment turn and switch
            _state.TurnCount++;
            _state.CurrentPlayer = _state.CurrentPlayer == _state.PlayerA ? _state.PlayerB : _state.PlayerA;

            // If the new current player is a bot, have it play immediately
            if (_state.CurrentPlayer.IsBot)
            {
                // bot shoots at PlayerA
                var botMove = ChooseBotMove(_state.PlayerA.Board);
                var botResult = FireAt(_state.PlayerA.Board, botMove.X, botMove.Y);
                if (botResult.processed)
                    _state.TurnCount++;

                // after bot's move, switch back to human player (PlayerA)
                _state.CurrentPlayer = _state.PlayerA;

                return Json(new
                {
                    turnCount = _state.TurnCount,
                    nextPlayer = _state.CurrentPlayer.Name,
                    botX = botMove.X,
                    botY = botMove.Y,
                    botHit = botResult.hit,
                    botSunk = botResult.sunk,
                    botGameOver = _state.PlayerA.Board.IsAllShipsSunk
                });
            }

            return Json(new
            {
                turnCount = _state.TurnCount,
                nextPlayer = _state.CurrentPlayer.Name
            });
        }

        [HttpPost]
        public IActionResult Fire(int x, int y)
        {
            if (_state == null)
                NewGameInternal(false);

            var player = _state.CurrentPlayer; // current shooter
            var opponent = player == _state.PlayerA ? _state.PlayerB : _state.PlayerA;
            var result = FireAt(opponent.Board, x, y);

            // if the shot was processed (not a repeated click), increment turn count
            if (result.processed)
                _state.TurnCount++;

            // If opponent is bot, bot plays immediately
            if (opponent.IsBot)
            {
                if (!opponent.Board.IsAllShipsSunk && player == _state.PlayerA)
                {
                    // switch turn to bot
                    _state.CurrentPlayer = opponent;
                    // bot chooses move
                    var botMove = ChooseBotMove(_state.PlayerA.Board);
                    var botResult = FireAt(_state.PlayerA.Board, botMove.X, botMove.Y);
                    if (botResult.processed)
                        _state.TurnCount++;
                    // switch back to player
                    _state.CurrentPlayer = _state.PlayerA;

                    return Json(new
                    {
                        playerHit = result.hit,
                        playerSunk = result.sunk,
                        playerGameOver = opponent.Board.IsAllShipsSunk,
                        botX = botMove.X,
                        botY = botMove.Y,
                        botHit = botResult.hit,
                        botSunk = botResult.sunk,
                        botGameOver = _state.PlayerA.Board.IsAllShipsSunk,
                        turnCount = _state.TurnCount,
                        nextPlayer = _state.CurrentPlayer.Name
                    });
                }

                return Json(new
                {
                    playerHit = result.hit,
                    playerSunk = result.sunk,
                    playerGameOver = opponent.Board.IsAllShipsSunk,
                    turnCount = _state.TurnCount,
                    nextPlayer = _state.CurrentPlayer.Name
                });
            }

            // Human vs Human (hot-seat): after a processed shot switch current player and wait for other player
            if (result.processed)
            {
                // switch current player to opponent
                _state.CurrentPlayer = opponent;
            }

            return Json(new
            {
                playerHit = result.hit,
                playerSunk = result.sunk,
                playerGameOver = opponent.Board.IsAllShipsSunk,
                turnCount = _state.TurnCount,
                nextPlayer = _state.CurrentPlayer.Name
            });
        }

        // Internal helpers
        private void NewGameInternal(bool hotseat)
        {
            _state = new GameState
            {
                PlayerA = new Player { Name = "Player A", IsBot = false, Board = CreateEmptyBoard() },
                PlayerB = new Player { Name = hotseat ? "Player B" : "Bot", IsBot = !hotseat, Board = CreateEmptyBoard() },
                TurnCount = 0
            };
            _state.CurrentPlayer = _state.PlayerA;

            // place ships
            PlaceShipsRandom(_state.PlayerA.Board);
            PlaceShipsRandom(_state.PlayerB.Board);
        }

        private Board CreateEmptyBoard()
        {
            return new Board
            {
                Size = new int[Rows, Cols],
                Ships = new List<Ship>(),
                ShotsFired = new List<Coordinate>()
            };
        }

        private void PlaceShipsRandom(Board board)
        {
            board.Ships.Clear();

            for (int i = 0; i < ShipLengths.Length; i++)
            {
                var length = ShipLengths[i];
                var ship = new Ship { Name = $"Ship{i + 1}", Length = length, Positions = new List<Coordinate>() };

                bool placed = false;
                int attempts = 0;
                while (!placed && attempts < 500)
                {
                    attempts++;
                    bool horizontal = _rand.Next(2) == 0;
                    int startX = _rand.Next(Rows);
                    int startY = _rand.Next(Cols);

                    var positions = new List<Coordinate>();
                    for (int p = 0; p < length; p++)
                    {
                        int nx = startX + (horizontal ? 0 : p);
                        int ny = startY + (horizontal ? p : 0);

                        if (nx < 0 || nx >= Rows || ny < 0 || ny >= Cols)
                        {
                            positions.Clear();
                            break;
                        }

                        positions.Add(new Coordinate { X = nx, Y = ny, IsHit = false });
                    }

                    if (positions.Count != length) continue;

                    // check overlap with existing ships
                    bool overlap = positions.Any(pos => board.Ships.Any(s => s.Positions.Any(sp => sp.X == pos.X && sp.Y == pos.Y)));
                    if (overlap) continue;

                    ship.Positions = positions;
                    board.Ships.Add(ship);
                    placed = true;
                }

                if (!placed)
                {
                    // as fallback, clear and try again from scratch
                    board.Ships.Clear();
                    i = -1; // will become 0 after loop increment
                }
            }
        }

        private (bool hit, bool sunk, bool processed) FireAt(Board board, int x, int y)
        {
            // ignore if already shot
            if (board.ShotsFired.Any(s => s.X == x && s.Y == y))
                return (false, false, false);

            var coord = new Coordinate { X = x, Y = y, IsHit = true };
            board.ShotsFired.Add(new Coordinate { X = x, Y = y, IsHit = true });

            foreach (var ship in board.Ships)
            {
                var match = ship.Positions.FirstOrDefault(p => p.X == x && p.Y == y);
                if (match != null)
                {
                    match.IsHit = true;
                    return (true, ship.IsSunk, true);
                }
            }

            return (false, false, true);
        }

        private Coordinate ChooseBotMove(Board playerBoard)
        {
            // Prefer adjacent cells to previous hits (simple heuristics)
            var hitPositions = playerBoard.ShotsFired.Where(s => playerBoard.Ships.Any(ship => ship.Positions.Any(p => p.X == s.X && p.Y == s.Y && p.IsHit))).ToList();
            var potential = new List<Coordinate>();

            foreach (var hit in hitPositions)
            {
                var neighbors = new[] {
                    new Coordinate{X=hit.X-1,Y=hit.Y},
                    new Coordinate{X=hit.X+1,Y=hit.Y},
                    new Coordinate{X=hit.X,Y=hit.Y-1},
                    new Coordinate{X=hit.X,Y=hit.Y+1}
                };

                foreach (var n in neighbors)
                {
                    if (n.X >= 0 && n.X < Rows && n.Y >= 0 && n.Y < Cols)
                    {
                        if (!playerBoard.ShotsFired.Any(s => s.X == n.X && s.Y == n.Y))
                            potential.Add(n);
                    }
                }
            }

            if (potential.Count > 0)
                return potential[_rand.Next(potential.Count)];

            // otherwise choose random unshot cell
            var unshot = new List<Coordinate>();
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    if (!playerBoard.ShotsFired.Any(s => s.X == i && s.Y == j))
                        unshot.Add(new Coordinate { X = i, Y = j });

            if (unshot.Count == 0)
                return new Coordinate { X = 0, Y = 0 };

            return unshot[_rand.Next(unshot.Count)];
        }
    }
}
