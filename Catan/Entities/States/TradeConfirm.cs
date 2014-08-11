using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities.States
{
    public class TradeConfirm : AbstractState
    {

        private Player _player;
        private Player _playerRecipient;

        private int _brickAmount;
        private int _oreAmount;
        private int _wheatAmount;
        private int _woodAmount;
        private int _woolAmount;
        private bool _recipientHasEnoughResources;

        public TradeConfirm(string playerID, string recipientID, Board board, int brickAmount, int oreAmount, int wheatAmount, int woodAmount, int woolAmount, bool recipientHasEnoughResources)
            : base(board, Entities.StateType.TRADE_CONFIRM, playerID)
        {
            _player = board.GetPlayerByID(playerID);
            _playerRecipient = board.GetPlayerByID(recipientID);

            _currentText = _player.Name + ", is waiting for " + _playerRecipient.Name + " to confirm his/her trade request.";

            _brickAmount = brickAmount;
            _oreAmount = oreAmount;
            _wheatAmount = wheatAmount;
            _woodAmount = woodAmount;
            _woolAmount = woolAmount;

            _recipientHasEnoughResources = recipientHasEnoughResources;
        }

        public string TradeConfirmDetails
        {
            get { return BuildTradeConfirmDetails(); } 
        }

        public string PlayerID
        {
            get { return _player.ID; }
        }

        public bool RecipientHasEnoughResources
        {
            get { return _recipientHasEnoughResources; }
        }

        public string RecipientPlayerID
        {
            get { return _playerRecipient.ID; }
        }

        public override AbstractState Cancel(Dictionary<string, string> data)
        {
            return this;
        }

        private void BuildTradeConfirmString(int value, string resourceType, out string recipientGive, out string recipientTake)
        {
            recipientTake = string.Empty;
            recipientGive = string.Empty;

            if (value < 0)
            {
                recipientTake += value.ToString().Trim('-') + " " + resourceType + ", ";
            }
            else if (value > 0)
            {
                recipientGive += value.ToString() + " " + resourceType + ", ";
            }

        }

        private string BuildTradeConfirmDetails()
        {
            string details = "Do you accept {0} for {1}?";
            string recipientGive = string.Empty;
            string recipientTake = string.Empty;
            string recipientGiveTotal = string.Empty;
            string recipientTakeTotal = string.Empty;

            BuildTradeConfirmString(_brickAmount, "brick", out recipientGive, out recipientTake);
            recipientGiveTotal += recipientGive;
            recipientTakeTotal += recipientTake;

            BuildTradeConfirmString(_oreAmount, "ore", out recipientGive, out recipientTake);
            recipientGiveTotal += recipientGive;
            recipientTakeTotal += recipientTake;

            BuildTradeConfirmString(_woodAmount, "wood", out recipientGive, out recipientTake);
            recipientGiveTotal += recipientGive;
            recipientTakeTotal += recipientTake;

            BuildTradeConfirmString(_woolAmount, "wool", out recipientGive, out recipientTake);
            recipientGiveTotal += recipientGive;
            recipientTakeTotal += recipientTake;

            BuildTradeConfirmString(_wheatAmount, "wheat", out recipientGive, out recipientTake);
            recipientGiveTotal += recipientGive;
            recipientTakeTotal += recipientTake;

            recipientGiveTotal = FormatString(recipientGiveTotal);
            recipientTakeTotal = FormatString(recipientTakeTotal);

            return string.Format(details, recipientTakeTotal, recipientGiveTotal);
        }

        private string FormatString(string value)
        {
            value = value.Trim();
            value = value.Trim(',');
            
            int commaCount = 0;
            foreach (char c in value)
            {
                if (c == ',')
                    commaCount++;
            }

            if (commaCount == 1)

            {
                return value.Replace(",", " and");
            }
            else if (commaCount > 1)
            {
                int lastIndex = value.LastIndexOf(',');
                return value.Substring(0, lastIndex) + " and " + value.Substring(lastIndex + 1, value.Length - lastIndex-1);
            }

            return value;
        }

        public override bool IsInputValid(Dictionary<string, string> data, out string message)
        {
            if (!data.Keys.Contains("confirm"))
            {
                message = "State/Sync error";
                return false;
            }
            else if (_playerRecipient.ID != data["recipientPlayerID"].ToString()) 
            {
                message = "This player is not permitted to perform this action right now";
                return false;
            }            
            else if (data["confirm"].ToString() != "yes" && data["confirm"].ToString() != "no")
            {
                message = "Invalid trade confirmation response";
            }

            message = "success";
            return true; 
        }

        private void ApplyTrade()
        {
            _player.Brick += _brickAmount;
            _player.Ore += _oreAmount;
            _player.Wheat += _wheatAmount;
            _player.Wood += _woodAmount;
            _player.Wool += _woolAmount;

            _playerRecipient.Brick += (-1 * _brickAmount);
            _playerRecipient.Ore += (-1 * _oreAmount);
            _playerRecipient.Wheat += (-1 * _wheatAmount);
            _playerRecipient.Wood += (-1 * _woodAmount);
            _playerRecipient.Wool += (-1 * _woolAmount);

        }

        public override AbstractState ProcessInputData(Dictionary<string, string> data)
        {
            string errorMessage = string.Empty;

            try
            {
                if (!IsInputValid(data, out errorMessage))
                    return this;

                string response = data["confirm"].ToString();

                if (response == "yes")
                {
                    ApplyTrade();
                    _nextState.HistoryText = _player.Name + " and " + _playerRecipient.Name + " have conducted a trade.";
                }

            }
            catch (Exception e)
            {
            }

            if (_nextState != null)
                return _nextState;
            return this;
        }
    }
}
