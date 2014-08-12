using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities.States
{
    public class PlayerSelectCity : AbstractState
    {
        private string _playerID;

        public PlayerSelectCity(string playerID, Board board)
            : base(board, Entities.StateType.PLAYER_SELECT_CITY, playerID)
        {
            _playerID = playerID;
            _currentText = board.GetPlayerByID(playerID).Name + ", is building a city.";
        }

        public string PlayerID
        {
            get { return _playerID; }
        }

        public override AbstractState Cancel(Dictionary<string, string> data)
        {
            Player player = _board.GetPlayerByID(_playerID);

            player.Ore += 3;

            player.Wheat += 2;

            return _nextState;
        }

        public override bool IsInputValid(Dictionary<string, string> data, out string message)
        {
            try
            {
                if (_playerID != data["playerID"].ToString())
                {
                    message = "A player is trying to establish a city when it is not their turn.";
                    return false;
                }
                else if (string.IsNullOrEmpty(data["settlementID"].ToString()) || data["settlementID"].ToString() == "-1")
                {
                    message = "SettlementID appears to be invalid.";
                    return false;
                }


                bool ownsASettlement = false;
                foreach (Settlement settlement in _board.Settlements)
                {
                    if (settlement.ID == Int32.Parse(data["settlementID"].ToString()) &&
                        !settlement.IsCity &&
                        settlement.PlayerID == _playerID)
                        ownsASettlement = true;
                }
                if (!ownsASettlement)
                {
                    message = "Player does not own a settlement";
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

        public override AbstractState ProcessInputData(Dictionary<string, string> data)
        {
            Player player = null;
            string errorMessage = string.Empty;
            int settlementID;
            try
            {
                // validate
                if(!IsInputValid(data, out errorMessage))
                    return this;

                player = _board.GetPlayerByID(_playerID);
                settlementID = Int32.Parse(data["settlementID"].ToString());
                //DrawCoordinate drawCoordinate = new DrawCoordinate(_board.SettlementCoordinates[settlementID - 1].X, _board.SettlementCoordinates[settlementID - 1].Y, 0);
                //drawCoordinate.X -= 30;
                //drawCoordinate.Y -= 28;

                Settlement settlement = _board.Settlements.Find(delegate(Settlement s) { return s.ID == settlementID; });

                settlement.UpgradeToCity(player);

                _nextState.HistoryText = player.Name + " built a city.";
                _nextState.Audio = Guid.NewGuid().ToString() + ":HRESCUE.wav";
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
    }
}
