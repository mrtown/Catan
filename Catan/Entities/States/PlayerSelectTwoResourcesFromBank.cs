using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities.States
{
    public class PlayerSelectTwoResourcesFromBank : AbstractState
    {
        private Player _player;

        public PlayerSelectTwoResourcesFromBank(string playerID, Board board)
            : base(board, Entities.StateType.PLAYER_SELECT_TWO_RESOURCES_FROM_BANK, playerID)
        {
            _player = board.GetPlayerByID(playerID);

            _currentText = _player.Name + " is selecting two resources of his/her choice - Year of Plenty.";
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
                else if (data["action"].ToString() != "PLAYER_TRADE")
                {
                    message = "This is an invalid action right now";
                    return false;
                }
                else if (data["recipientID"].ToString() != "bank")
                {
                    message = "The recipient must be the 'bank'";
                    return false;
                }

                if (!IsPlayerTakingExactlyTwoResources(data))
                {
                    message = "The player must take exactly two resources from the bank";
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
            string errorMessage = string.Empty;
            try
            {
                if (!IsInputValid(data, out errorMessage))
                    return this;

                AssignPlayerResources(data);
                
                _nextState.HistoryText = _player.Name + " has gained two resources - Year of Plenty.";
            }
            catch(Exception e)
            {
            
            }

            if (_nextState != null)
                return _nextState;
            return this;
        }

        public override AbstractState Cancel(Dictionary<string, string> data)
        {
            return this;
        }

        private void AssignPlayerResources(Dictionary<string, string> data)
        {
            // This method assumes IsPlayerTakingExactlyTwoResources has been called.
            int brick = Int32.Parse(data["brickAmount"].ToString());
            int ore = Int32.Parse(data["oreAmount"].ToString());
            int wheat = Int32.Parse(data["wheatAmount"].ToString());
            int wood = Int32.Parse(data["woodAmount"].ToString());
            int wool = Int32.Parse(data["woolAmount"].ToString());

            if (brick > 0)
                _player.Brick += brick;
            if (ore > 0)
                _player.Ore += ore;
            if (wheat > 0)
                _player.Wheat += wheat;
            if (wood > 0)
                _player.Wood += wood;
            if (wool > 0)
                _player.Wool += wool;
            
        }

        private bool IsPlayerTakingExactlyTwoResources(Dictionary<string, string> data)
        {            
            int brick = Int32.Parse(data["brickAmount"].ToString());
            int ore = Int32.Parse(data["oreAmount"].ToString());
            int wheat = Int32.Parse(data["wheatAmount"].ToString());
            int wood = Int32.Parse(data["woodAmount"].ToString());
            int wool = Int32.Parse(data["woolAmount"].ToString());

            int sum = 0;

            if (brick > 0)
                sum += brick;
            if (ore > 0)
                sum += ore;
            if (wheat > 0)
                sum += wheat;
            if (wood > 0)
                sum += wood;
            if (wool > 0)
                sum += wool;
            
            if (sum != 2)
                return false;
            
            return true;
        }
    }
}
