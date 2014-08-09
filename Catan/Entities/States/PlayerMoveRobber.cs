using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities.States
{
    public class PlayerMoveRobber : AbstractState
    {
        private string _playerID;

        public PlayerMoveRobber(string playerID, Board board)
            : base(board, StateType.PLAYER_MOVE_ROBBER)
        {
            _playerID = playerID;
            _currentText = board.GetPlayerByID(playerID).Name + ", is moving the Robber.";
        }

        public string PlayerID
        {
            get { return _playerID; }
        }

        public override AbstractState Cancel(Dictionary<string, string> data)
        {
            return this;
        }

        public override bool IsInputValid(Dictionary<string, string> data, out string message)
        {
            try
            {
                Tile tileWithRobber = _board.Tiles.Where(t => t.Robber).First();

                if (_playerID != data["playerID"].ToString())
                {
                    message = "A player is trying to move the robber when it is not their turn.";
                    return false;
                }
                else if (string.IsNullOrEmpty(data["tileID"].ToString()) || data["tileID"].ToString() == "-1")
                {
                    message = "tileID appears to be invalid.";
                    return false;
                }
                else if (tileWithRobber.ID == Int32.Parse(data["tileID"].ToString()))
                {
                    message = "The Robber must be moved to a new location";
                    return false;                    
                }

            }
            catch (Exception e)
            {
                message = e.Message;
                return false;
            }

            message = "success";
            return true;            
        }

        public List<Settlement> GetAdjacentEnemySettlements(int tileID)
        {
            // returns adjacent settlements belonging
            // to players with at least 1 resource.

            List<Settlement> adjacentSettlements = new List<Settlement>();

            foreach (Settlement settlement in _board.Settlements)
                if (_board.SettlementTileAdjacency[settlement.ID, tileID] 
                    && _playerID != settlement.PlayerID
                    && _board.GetPlayerByID(settlement.PlayerID).TotalNumberOfResources > 0
                    && !adjacentSettlements.Exists(delegate(Settlement s) { return s.PlayerID == settlement.PlayerID; }))
                        adjacentSettlements.Add(settlement);

            return adjacentSettlements;
        }

        public override AbstractState ProcessInputData(Dictionary<string, string> data)
        {
            string errorMessage = string.Empty;
            int tileID;
            Player player = null;
            try
            {
                if (!IsInputValid(data, out errorMessage))
                    return this;

                player = _board.GetPlayerByID(_playerID);

                tileID = Int32.Parse(data["tileID"].ToString());

                _board.UpdateRobber(tileID);

                List<Settlement> adjacentSettlements = GetAdjacentEnemySettlements(tileID);

                if (adjacentSettlements.Count > 0)
                    _nextState = DetermineNextState(adjacentSettlements, tileID);
                
                _nextState.HistoryText = player.Name + " moved the Robber.";                 
            }
            catch (Exception e)
            {
            }

            if (_nextState != null)
            {
                return _nextState;
            }

            return this;
        }

        private AbstractState DetermineNextState(List<Settlement> settlements, int tileID)
        {
            if (settlements.Count == 1)
            {
                Player player = _board.GetPlayerByID(settlements[0].PlayerID);
                if (player.TotalNumberOfResources != 0)
                {
                    Player currentPlayer = _board.GetPlayerByID(_playerID);
                    player.PayPlayerOneRandomResource(currentPlayer);
                }

                return _nextState;
            }
            else
            {
                PlayerSelectPlayer playerState = new PlayerSelectPlayer(_playerID, _board, settlements);
                playerState.NextState = _nextState;
                return playerState;
            }
        }        
    }
}
