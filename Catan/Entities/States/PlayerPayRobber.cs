using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities.States
{
    public class PlayerPayRobber : AbstractState
    {
        private Player _player;
        private int _amountToPay;

        public PlayerPayRobber(string playerID, Board board)
            : base(board, StateType.PLAYER_PAY_ROBBER)
        {
            _player = board.GetPlayerByID(playerID);
            _amountToPay = CalculateAmountToPay();
            _currentText = _player.Name + " is currently paying the Robber " + _amountToPay.ToString() + " resources.";
        }

        public string PlayerID
        {
            get { return _player.ID; }
        }
        

        public int AmountToPay
        {
            get { return _amountToPay; }
        }

        private int CalculateAmountToPay()
        {

            return (int)Math.Floor((decimal)_player.TotalNumberOfResources/2);
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
                else if (data["recipientID"].ToString() != "robber")
                {
                    message = "The recipient must be the 'robber'";
                    return false;
                }

                if (!IsPlayerPayingRobberExactAmount(data))
                {
                    message = "The player must pay the robber exactly half (rounding down) of his/her resources.";
                    return false;
                }
                if (!CanPlayerAffordThis(data))
                {
                    message = "The player can't afford this";
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
            message = "success";
            return true;
        }

        public override AbstractState Cancel(Dictionary<string, string> data)
        {
            return this;
        }

        private bool IsPlayerPayingRobberExactAmount(Dictionary<string, string> data)
        {
            int totalAmountPaid = 0;
            
            int brick = Int32.Parse(data["brickAmount"].ToString());
            int ore = Int32.Parse(data["oreAmount"].ToString());
            int wheat = Int32.Parse(data["wheatAmount"].ToString());
            int wood = Int32.Parse(data["woodAmount"].ToString());
            int wool = Int32.Parse(data["woolAmount"].ToString());

            if (brick < 0)
                totalAmountPaid += brick * -1;
            if (ore < 0)
                totalAmountPaid += ore * -1;
            if (wheat < 0)
                totalAmountPaid += wheat * -1;
            if (wood < 0)
                totalAmountPaid += wood * -1;
            if (wool < 0)
                totalAmountPaid += wool * -1;

            if (totalAmountPaid == _amountToPay)
                return true;
            else
                return false;

        }

        private bool CanPlayerAffordThis(Dictionary<string, string> data)
        {
            int brick = Int32.Parse(data["brickAmount"].ToString());
            int ore = Int32.Parse(data["oreAmount"].ToString());
            int wheat = Int32.Parse(data["wheatAmount"].ToString());
            int wood = Int32.Parse(data["woodAmount"].ToString());
            int wool = Int32.Parse(data["woolAmount"].ToString());

            if (brick < 0)
                if (_player.Brick < (brick * -1))
                    return false;
            if (ore < 0)
                if (_player.Ore < (ore * -1))
                    return false;
            if (wheat < 0)
                if (_player.Wheat < (wheat * -1))
                    return false;
            if (wood < 0)
                if (_player.Wood < (wood * -1))
                    return false;
            if (wool < 0)
                if (_player.Wool < (wool * -1))
                    return false;

            return true;
        }

        public override AbstractState ProcessInputData(Dictionary<string, string> data)
        {
            string errorMessage = string.Empty;
            try
            {
                if (!IsInputValid(data, out errorMessage))
                    return this;



                DebitPlayer(data);

                _nextState.HistoryText = _player.Name + " has paid the robber " + _amountToPay.ToString() + " resources.";
            }
            catch (Exception e)
            {
            }

            if (_nextState != null)
                return _nextState;
            return this;
        }

        private void DebitPlayer(Dictionary<string, string> data)
        {
            int brick = Int32.Parse(data["brickAmount"].ToString());
            int ore = Int32.Parse(data["oreAmount"].ToString());
            int wheat = Int32.Parse(data["wheatAmount"].ToString());
            int wood = Int32.Parse(data["woodAmount"].ToString());
            int wool = Int32.Parse(data["woolAmount"].ToString());

            if (brick < 0)
                _player.Brick += brick;
            if (ore < 0)
                _player.Ore += ore;
            if (wheat < 0)
                _player.Wheat += wheat;
            if (wood < 0)
                _player.Wood += wood;
            if (wool < 0)
                _player.Wool += wool;
        }

    }
}
