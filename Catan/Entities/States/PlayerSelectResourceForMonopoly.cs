using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities.States
{
    public class PlayerSelectResourceForMonopoly : AbstractState
    {
        private Player _player;
        public PlayerSelectResourceForMonopoly(string playerID, Board board)
            : base(board, Entities.StateType.PLAYER_SELECT_RESOURCE_FOR_MONOPOLY, playerID)
        {
            _player = board.GetPlayerByID(playerID);

            _currentText = _player.Name + " is selecting a resource of his/her choise - Monopoly.";
        }

        public string PlayerID
        {
            get { return _player.ID; }
        }

        public override bool IsInputValid(Dictionary<string, string> data, out string message)
        {

            try
            {

                if (data["playerID"].ToString() != _player.ID)
                {
                    message = "This player is not allowed to perform an action right now.";
                    return false;
                }
                if (string.IsNullOrEmpty(data["resource"].ToString()))
                {
                    message = "A resource (Monopoly) was not selected";
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
            string errorMessage = string.Empty;
            try
            {
                if (!IsInputValid(data, out errorMessage))
                    return this;

                ApplyMonopoly(data);

                _nextState.HistoryText = _player.Name + " has applied a " + data["resource"].ToString() + " monopoly.";
            }
            catch (Exception e)
            { }
            if (_nextState != null)
                return _nextState;
            return this;
        }

        public override AbstractState Cancel(Dictionary<string, string> data)
        {
            return this;
        }

        private void ApplyMonopoly(Dictionary<string, string> data)
        {
            string resource = data["resource"].ToString();

            foreach (Player player in _board.Players)
            {
                if (player.ID == _player.ID)
                    continue;

                if (resource == "brick")
                {
                    _player.Brick += player.Brick;
                    player.Brick = 0;
                }
                else if (resource == "wood")
                {
                    _player.Wood += player.Wood;
                    player.Wood = 0;
                }
                else if (resource == "wheat")
                {
                    _player.Wheat += player.Wheat;
                    player.Wheat = 0;                
                }
                else if (resource == "wool")
                {
                    _player.Wool += player.Wool;
                    player.Wool = 0;
                }
                else if (resource == "ore")
                {
                    _player.Ore += player.Ore;
                    player.Ore = 0;
                }
            }
        }
    }
}
