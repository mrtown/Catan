using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Entities;

namespace Catan.Entities.States
{
    public class PlayerSelectPlayer : AbstractState
    {
        private string _playerID;
        List<Settlement> _settlements;
        public PlayerSelectPlayer(string playerID, Board board, List<Settlement> settlements)
            : base(board, StateType.PLAYER_SELECT_PLAYER, playerID)
        {
            _playerID = playerID;
            _settlements = settlements;
            _currentText = board.GetPlayerByID(playerID).Name + " is selecting which player to rob.";
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
                string selectedPlayer = data["selectedPlayer"].ToString();
                
                if (_playerID != data["playerID"].ToString())
                {
                    message = "This player is not permitted to perform this action.";
                    return false;
                }
                else if (selectedPlayer != "1" &&
                         selectedPlayer != "2" &&
                         selectedPlayer != "3" &&
                         selectedPlayer != "4")
                {
                    message = "The selected player ID appears to be invalid.";
                    return false;
                }
                else if (!SelectedPlayerIsAdjacent(selectedPlayer))
                {
                    message = "The selected player is not adjacent";
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
            Player robbedPlayer = null;
            string errorMessage = string.Empty;

            try
            {
                if (!IsInputValid(data, out errorMessage))
                    return this;

                player = _board.GetPlayerByID(_playerID);
                robbedPlayer = _board.GetPlayerByNumber(Int32.Parse(data["selectedPlayer"].ToString()));

                robbedPlayer.PayPlayerOneRandomResource(player);

                _nextState.HistoryText = player.Name + " has robbed " + robbedPlayer.Name + " one resource.";
            }
            catch (Exception e)
            {
            
            }

            if (_nextState != null)
                return _nextState;
            return this;
        }

        private bool SelectedPlayerIsAdjacent(string selectedPlayer)
        {
            Player p = _board.GetPlayerByNumber(Int32.Parse(selectedPlayer));
            foreach (Settlement settlement in _settlements)
                if (p.ID == settlement.PlayerID)
                    return true;
            return false;
        }
    }
}
