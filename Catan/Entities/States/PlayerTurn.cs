using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities.States
{
    [Serializable]
    public class PlayerTurn : AbstractState
    {
        private string _playerID;
        private bool _hasAlreadyRolled;
        private bool _hasAlreadyPlayDevelopmentCard;
        private static Random _randomGenerator = new Random(DateTime.Now.Millisecond);

        public PlayerTurn(string playerID, Board board)
            : base(board, Entities.StateType.PLAYER_TURN, playerID)
        {
            _playerID = playerID;
            _hasAlreadyRolled = false;
            _hasAlreadyPlayDevelopmentCard = false;
            _currentText = "It is currently " + board.GetPlayerByID(playerID).Name + "'s turn.";
        }

        public override AbstractState Cancel(Dictionary<string, string> data)
        {
            return this;
        }

        public bool HasAlreadyPlayDevelopmentCard
        {
            get { return _hasAlreadyPlayDevelopmentCard; }
            set { _hasAlreadyPlayDevelopmentCard = value; }
        }

        public bool HasAlreadyRolled
        {
            get { return _hasAlreadyRolled; }
            set { _hasAlreadyRolled = value; }
        }

        public string PlayerID
        {
            get { return _playerID; }
        }

        public override bool IsInputValid(Dictionary<string, string> data, out string message)
        {
            try
            {
                if ((data["action"].ToString() == Entities.StateType.PLAYER_TRADE.ToString() ||
                     data["action"].ToString() == Entities.StateType.PLAYER_BUILD.ToString() ||
                     data["action"].ToString() == Entities.StateType.PLAYER_PASS.ToString()) &&
                     !_hasAlreadyRolled)
                {
                    message = "You must roll before you can perform that action.";
                    _board.Message = message; 
                    return false;
                }

                if (data["action"].ToString() == Entities.StateType.PLAYER_PLAY_CARD.ToString() && _hasAlreadyPlayDevelopmentCard)
                {
                    message = "You have already played a card. You can only play 1 card per tern.";
                    _board.Message = message;
                    return false;                    
                }

                if (_playerID != data["playerID"].ToString())
                {
                    message = "This player is not permitted to perform an action right now";
                    return false;
                }
                else if (_hasAlreadyRolled && data["action"].ToString() == Entities.StateType.PLAYER_ROLL.ToString())
                {
                    message = "The player has already rolled this turn";
                    return false;
                }
                else if (!_hasAlreadyRolled && data["action"].ToString() == Entities.StateType.PLAYER_PASS.ToString())
                {
                    message = "You must roll before you pass your turn.";
                    return false;
                }
                else if (data["action"].ToString() == Entities.StateType.PLAYER_TRADE.ToString())
                {                    
                    int brick = Int32.Parse(data["brickAmount"].ToString());
                    int ore = Int32.Parse(data["oreAmount"].ToString());
                    int wheat = Int32.Parse(data["wheatAmount"].ToString());
                    int wood = Int32.Parse(data["woodAmount"].ToString());
                    int wool = Int32.Parse(data["woolAmount"].ToString());

                    if (brick == 0 &&
                        ore == 0 &&
                        wheat == 0 &&
                        wood == 0 &&
                        wool == 0)
                    {
                        message = "Invalid trade details";
                        return false;
                    }
                        
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

        private int Roll()
        {
            int die1 = _randomGenerator.Next(1, 7);
            int die2 = _randomGenerator.Next(1, 7);
            int roll = die1 + die2;

            Player player = null;

            foreach (Tile tile in _board.Tiles)
            {
                if (tile.Frequency != null && tile.Frequency.FrequencyValue == roll && !tile.Robber)
                {
                    foreach(Settlement settlement in _board.Settlements)
                    {
                        if (_board.SettlementTileAdjacency[settlement.ID, tile.ID])
                        {
                            player = _board.GetPlayerByID(settlement.PlayerID);
                            if (!settlement.IsCity)
                                AddResource(player, tile.TileType, 1);
                            else
                                AddResource(player, tile.TileType, 2);
                        }
                    }
                    
                }
            }

            _hasAlreadyRolled = true;

            return roll;
        }

        private void AddResource(Player player, Enums.TileType tileType, int numberOfResources)
        {
            if (tileType == Enums.TileType.Brick)
                player.Brick += numberOfResources;
            else if (tileType == Enums.TileType.Ore)
                player.Ore += numberOfResources;
            else if (tileType == Enums.TileType.Wheat)
                player.Wheat += numberOfResources;
            else if (tileType == Enums.TileType.Wood)
                player.Wood += numberOfResources;
            else if (tileType == Enums.TileType.Wool)
                player.Wool += numberOfResources;


        }

        private bool DoesPlayerHaveThisCard(Player player, string developmentCard)
        {
            return player.HeldCards.Exists(delegate(AbstractDevelopmentCard card) { return card.Type.ToString() == developmentCard && card.IsPlayable; });            
        }


        public bool HasPlayerReachedMaxSettlements()
        {
            int c = 0;
            foreach (Settlement settlement in _board.Settlements)
                if (settlement.PlayerID == _playerID && !settlement.IsCity)
                    c++;
            if (c < 5)
                return false;
            else
                return true;
        }

        public bool HasPlayerReachedMaxRoads()
        {
            int c = 0;
            foreach (Road road in _board.Roads)
                if (road.PlayerID == _playerID)
                    c++;
            if (c < 15)
                return false;
            else
                return true;
        }

        public bool HasPlayerReachedMaxCities()
        {
            int c = 0;
            foreach (Settlement settlement in _board.Settlements)
                if (settlement.PlayerID == _playerID && settlement.IsCity)
                    c++;
            if (c < 4)
                return false;
            else
                return true;
        }  

        private bool DoesPlayerHaveEnoughResources(Player player, string buildItem)
        {
            
            if (buildItem == "settlement")
            {
                if (player.Brick >= 1 &&
                    player.Wheat >= 1 &&
                    player.Wood >= 1 &&
                    player.Wool >= 1 &&
                    !HasPlayerReachedMaxSettlements())
                    return true;
                else
                    return false;
            }
            else if (buildItem == "city")
            {
                if (player.Ore >= 3 && player.Wheat >= 2 && !HasPlayerReachedMaxCities())
                {
                    return _board.Settlements.Exists(delegate(Settlement s) { return s.PlayerID == player.ID && !s.IsCity; });
                }                    
                else
                    return false;
            }
            else if (buildItem == "road")
            {
                if (player.Brick >= 1 &&
                    player.Wood >= 1 && 
                    !HasPlayerReachedMaxRoads())
                    return true;
                else
                    return false;
            }
            else if (buildItem == "developmentCard")
            {
                if (player.Wool >= 1 &&
                    player.Wheat >= 1 &&
                    player.Ore >= 1)
                    return true;
                else
                    return false;
            }

            return false;
        }

        private void DebitPlayer(Player player, string buildItem)
        {
            if (buildItem == "settlement")
            {
                player.Brick--;
                player.Wood--;
                player.Wheat--;
                player.Wool--;
            }
            else if (buildItem == "city")
            {
                player.Ore -= 3;
                player.Wheat -= 2;
            }
            else if (buildItem == "road")
            {
                player.Brick--;
                player.Wood--;
            }
            else if (buildItem == "developmentCard")
            {
                player.Ore--;
                player.Wheat--;
                player.Wool--;
            }           
        }

        private AbstractState PlayCard(string developmentCard, Player player)
        {
            AbstractDevelopmentCard card;
            AbstractState ptr = null;

            if (DoesPlayerHaveThisCard(player, developmentCard))
            {
                card = player.GetDevelopmentCard(developmentCard);
                AbstractState state = card.Play(player, _board);

                _historyText = player.Name + " has played a " + developmentCard + " card.";

                _hasAlreadyPlayDevelopmentCard = true;

                if (state != null)
                {
                    ptr = state;
                    while (ptr.NextState != null)
                        ptr = ptr.NextState;                        

                    ptr.NextState = this;
                    
                    return state;                    
                }
            }
            
            return this;
        }

        private bool IsEnoughOfGivenResource(int transactionAmount, int playerAmount, int recipientAmount, out bool recipientHasEnoughResources)
        {
            recipientHasEnoughResources = true;

            if (transactionAmount < 0)
            {
                if ((transactionAmount + playerAmount) < 0)
                    return false;
            }
            else if (transactionAmount > 0)
            {
                if (((transactionAmount * -1) + recipientAmount) < 0)
                {
                    recipientHasEnoughResources = false;
                    //return false;
                }
            }

            return true;
        }

        private bool DoPlayersHaveEnoughResourcesToTrade(string recipientID, int brickAmount, int oreAmount, int wheatAmount, int woodAmount, int woolAmount, out bool recipientHasEnoughResources)
        {
            Player currentPlayer = _board.GetPlayerByID(_playerID);
            Player recipientPlayer = _board.GetPlayerByID(recipientID);
            bool temp = true;

            if (!IsEnoughOfGivenResource(brickAmount, currentPlayer.Brick, recipientPlayer.Brick, out recipientHasEnoughResources))
                return false;
            if (!recipientHasEnoughResources)
                temp = false;

            if (!IsEnoughOfGivenResource(oreAmount, currentPlayer.Ore, recipientPlayer.Ore, out recipientHasEnoughResources))
                return false;
            if (!recipientHasEnoughResources)
                temp = false;

            if (!IsEnoughOfGivenResource(wheatAmount, currentPlayer.Wheat, recipientPlayer.Wheat, out recipientHasEnoughResources))
                return false;
            if (!recipientHasEnoughResources)
                temp = false;

            if (!IsEnoughOfGivenResource(woodAmount, currentPlayer.Wood, recipientPlayer.Wood, out recipientHasEnoughResources))
                return false;
            if (!recipientHasEnoughResources)
                temp = false;

            if (!IsEnoughOfGivenResource(woolAmount, currentPlayer.Wool, recipientPlayer.Wool, out recipientHasEnoughResources))
                return false;
            if (!recipientHasEnoughResources)
                temp = false;

            recipientHasEnoughResources = temp;
            return true;
        }

        private void ApplyTradeWithBank(int brickAmount, int oreAmount, int wheatAmount, int woodAmount, int woolAmount)
        {
            Player player = _board.GetPlayerByID(_playerID);

            player.Brick += brickAmount;
            player.Ore += oreAmount;
            player.Wheat += wheatAmount;
            player.Wood += woodAmount;
            player.Wool += woolAmount;

            //_playerRecipient.Brick += (-1 * _brickAmount);
            //_playerRecipient.Ore += (-1 * _oreAmount);
            //_playerRecipient.Wheat += (-1 * _wheatAmount);
            //_playerRecipient.Wood += (-1 * _woodAmount);
            //_playerRecipient.Wool += (-1 * _woolAmount);

        }

        private void ProcessPlayerTradeWithBank(int brickAmount, int oreAmount, int wheatAmount, int woodAmount, int woolAmount)
        {
            int totalResourcesPurchasedFromBank = 0;
            
            if (brickAmount > 0)
                totalResourcesPurchasedFromBank += brickAmount;
            if (oreAmount > 0)
                totalResourcesPurchasedFromBank += oreAmount;
            if (wheatAmount > 0)
                totalResourcesPurchasedFromBank += wheatAmount;
            if (woodAmount > 0)
                totalResourcesPurchasedFromBank += woodAmount;
            if (woolAmount > 0)
                totalResourcesPurchasedFromBank += woolAmount;

            int totalCredit = 0;
            bool valid = true;
            totalCredit += CalculateCredit(brickAmount, "brick", out valid);                        
            if (!valid)
            {
                _board.Message = "The terms of the trade with the Bank are invalid.";
                return;
            }

            totalCredit += CalculateCredit(oreAmount, "ore", out valid);
            if (!valid)
            {
                _board.Message = "The terms of the trade with the Bank are invalid.";
                return;
            }
            
            totalCredit += CalculateCredit(wheatAmount, "wheat", out valid);
            if (!valid)
            {
                _board.Message = "The terms of the trade with the Bank are invalid.";
                return;
            }

            totalCredit += CalculateCredit(woodAmount, "wood", out valid);
            if (!valid)
            {
                _board.Message = "The terms of the trade with the Bank are invalid.";
                return;
            }

            totalCredit += CalculateCredit(woolAmount, "wool", out valid);
            if (!valid)
            {
                _board.Message = "The terms of the trade with the Bank are invalid.";
                return;
            }

            if ((totalCredit == totalResourcesPurchasedFromBank) && totalCredit != 0)
            {
                if (PlayerCanAffordToPayBank(brickAmount, oreAmount, wheatAmount, woodAmount, woolAmount))
                {
                    ApplyTradeWithBank(brickAmount, oreAmount, wheatAmount, woodAmount, woolAmount);
                    _historyText = _board.GetPlayerByID(_playerID).Name + " has conducted a trade with the bank";
                }
                else
                {
                    _board.Message = "You can't afford that.";
                }
            }
            else
            {
                _board.Message = "The terms of the trade with the Bank are invalid.";
            }
        
        }

        private bool PlayerCanAffordToPayBank(int brickAmount, int oreAmount, int wheatAmount, int woodAmount, int woolAmount)
        {
            Player player = _board.GetPlayerByID(_playerID);

            if (player.Brick + brickAmount >= 0 &&
                player.Ore + oreAmount >= 0 &&
                player.Wheat + wheatAmount >= 0 &&
                player.Wood + woodAmount >= 0 &&
                player.Wool + woolAmount >= 0)
                return true;

            return false;
        }

        private int CalculateCredit(int amount, string type, out bool valid)
        {                                    
            //Player player = _board.GetPlayerByID(_playerID);
            int factor;
            int value;

            valid = true;

            if (_board.PlayerHasPortOfType(_playerID, type))
            {
                factor = 2;
                if (amount % factor == 0 && amount < 0)
                {
                    return -1*(amount / factor);
                }
            }
            else if (_board.PlayerHasAGeneralPort(_playerID))
            {
                factor = 3;
                if (amount % factor == 0 && amount < 0)
                {
                    return -1*(amount / factor); 
                }
            }
            else
            {
                factor = 4;
                if (amount % factor == 0 && amount < 0)
                {
                    return -1 * (amount / factor);
                }            
            }

            if (amount < 0)
                valid = false;

            return 0;
        }

        private void ProcessPlayerTradeWithRobber(int brickAmount, int oreAmount, int wheatAmount, int woodAmount, int woolAmount)
        {


        }

        private AbstractState Trade(string recipientID, int brickAmount, int oreAmount, int wheatAmount, int woodAmount, int woolAmount)
        {
            bool recipientHasEnoughResources = true;

            if (recipientID == "bank")
            {
                ProcessPlayerTradeWithBank(brickAmount, oreAmount, wheatAmount, woodAmount, woolAmount);
            }
            else if (recipientID != "robber")// any player
            {
                if (DoPlayersHaveEnoughResourcesToTrade(recipientID, brickAmount, oreAmount, wheatAmount, woodAmount, woolAmount, out recipientHasEnoughResources))
                {
                    AbstractState state = new TradeConfirm(_playerID, recipientID, _board, brickAmount, oreAmount, wheatAmount, woodAmount, woolAmount, recipientHasEnoughResources);
                    state.NextState = this;
                    return state;
                }
                else
                {
                    _board.Message = "You do not have enough resources for that trade.";
                }
            }
            return this;
        }

        private AbstractState Build(string buildItem, Player player)
        {
            if (buildItem == "developmentCard" && _board.DevelopmentCardDeck.Count < 1)
                return this;

            if (DoesPlayerHaveEnoughResources(player, buildItem))
            {
                DebitPlayer(player, buildItem);

                if (buildItem == "settlement")
                {
                    PlayerSelectSettlement playerSelectSettlement = new PlayerSelectSettlement(player.ID, _board, false, false);
                    playerSelectSettlement.NextState = this;
                    return playerSelectSettlement;
                }
                else if (buildItem == "city")
                {
                    PlayerSelectCity playerSelectCity = new PlayerSelectCity(player.ID, _board);
                    playerSelectCity.NextState = this;
                    return playerSelectCity;
                }
                else if (buildItem == "road")
                {
                    PlayerSelectRoad playerSelectroad = new PlayerSelectRoad(player.ID, _board, true, false);
                    playerSelectroad.NextState = this;
                    return playerSelectroad;
                }
                else if (buildItem == "developmentCard")
                {
                    player.HeldCards.Add(_board.GetRandomDevelopmentCard());
                    _historyText = player.Name + " has purchased a Development Card.";
                    return this;
                }   
            }
            return this;
        }

        private AbstractState ProcessRolledSeven()
        {
            List<PlayerPayRobber> players = new List<PlayerPayRobber>();
            
            foreach (Player player in _board.Players)
                if (player.TotalNumberOfResources > 7)
                    players.Add(new PlayerPayRobber(player.ID, _board));

            if (players.Count > 0)
            {
                for (int i = 0; i < players.Count - 1; i++)
                    players[i].NextState = players[i + 1];

                PlayerMoveRobber moveRobber = new PlayerMoveRobber(_playerID, _board);
                moveRobber.NextState = this;
                players[players.Count - 1].NextState = moveRobber;
                return players[0];
            }
            else
            {
                PlayerMoveRobber moveRobber = new PlayerMoveRobber(_playerID, _board);
                moveRobber.NextState = this;
                return moveRobber;
            }
               
        }

        public override AbstractState ProcessInputData(Dictionary<string, string> data)
        {
            Player player = null;
            string errorMessage = string.Empty;
            try
            {
                if (!IsInputValid(data, out errorMessage))
                    return this;

                player = _board.GetPlayerByID(_playerID);

                if (data["action"].ToString() == Entities.StateType.PLAYER_ROLL.ToString())
                {
                    int roll = Roll();
                    _historyText = player.Name + " has rolled a " + roll.ToString();

                    _board.Rolled = roll;

                    string audio = Guid.NewGuid().ToString() + ":dice.m4a";
                    if (_nextState != null)
                        _nextState.Audio = audio;
                    else
                        this.Audio = audio;
                    
                    if (roll == 7)
                    {
                        AbstractState s = ProcessRolledSeven();
                        if (s != null)
                        {
                            s.Audio = audio;
                            return s;
                        }                            
                    }
                }
                else if (data["action"].ToString() == Entities.StateType.PLAYER_PASS.ToString())
                {
                    player.SetAllDevelopmentCardsPlayable();
                    _board.Rolled = 0;
                    _nextState = new PlayerTurn(_board.GetNextPlayer(_playerID).ID, _board);
                    _nextState.HistoryText = player.Name + " has completed his/her turn.";
                    _nextState.Audio = Guid.NewGuid().ToString() + ":gogogo.wav";
                }
                else if (data["action"].ToString() == Entities.StateType.PLAYER_BUILD.ToString())
                {
                    _nextState = null;
                    return Build(data["buildItem"].ToString(), player);                       
                }
                else if (data["action"].ToString() == Entities.StateType.PLAYER_PLAY_CARD.ToString())
                {
                    _nextState = null;
                    return PlayCard(data["card"].ToString(), player);
                }
                else if (data["action"].ToString() == Entities.StateType.PLAYER_TRADE.ToString())
                {
                    _nextState = null;
                    return Trade(data["recipientID"].ToString(),
                                 Int32.Parse(data["brickAmount"].ToString()),
                                 Int32.Parse(data["oreAmount"].ToString()),
                                 Int32.Parse(data["wheatAmount"].ToString()),
                                 Int32.Parse(data["woodAmount"].ToString()),
                                 Int32.Parse(data["woolAmount"].ToString()));
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
