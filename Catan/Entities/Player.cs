using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Enums;

namespace Catan.Entities
{
    public class Player
    {
        private string _id;
        private string _name;
        private string _color;
        private int _playerNumber;

        private int _wood;
        private int _wool;
        private int _wheat;
        private int _brick;
        private int _ore;

        private int _score;
        private int _displayScore;
        private int _longestRoadLength;
        private int _numberOfHarbours;

        private bool _hasLargestArmy;
        private bool _hasLongestRoad;
        private bool _hasMostHarbours;

        private bool _hadLongestRoadDecreased;

        private int _mostRecentlyBuiltSettlementID;

        private Random _randomGenerator = new Random(DateTime.Now.Millisecond);

        private List<TradeRecipient> _tradeRecipients;

        private List<AbstractDevelopmentCard> _playedCards;
        private List<AbstractDevelopmentCard> _heldCards;


                

        public Player(string id, string name, string color, int playerNumber, List<TradeRecipient> tradeRecipients)
        {
            _id = id;
            _name = name;
            _color = color;
            _playerNumber = playerNumber;

            _wood = 0;
            _wool = 0;
            _wheat = 0;
            _brick = 0;
            _ore = 0;

            _score = 0;
            _longestRoadLength = 0;
            _numberOfHarbours = 0;

            _hasLongestRoad = false;
            _hasLargestArmy = false;
            _hasMostHarbours = false;
            _hadLongestRoadDecreased = false;

            _playedCards = new List<AbstractDevelopmentCard>();
            _heldCards = new List<AbstractDevelopmentCard>();

            _tradeRecipients = tradeRecipients;

            _mostRecentlyBuiltSettlementID = -1;
        }

        public void AddRoadBuildCardBackToHand()
        {
            DevelopmentCardType type = (DevelopmentCardType)Enum.Parse(typeof(DevelopmentCardType), "roadBuilding");
            AbstractDevelopmentCard card = _playedCards.Find(delegate(AbstractDevelopmentCard c) { return c.Type == type; });
            if (card != null)
            {
                _playedCards.Remove(card);
                _heldCards.Add(card);

            }
        }

        public void IncrementPlayerResourceByOne(TileType type)
        {
            switch (type)
            {
                case TileType.Brick:
                    _brick++;
                    break;
                case TileType.Ore:
                    _ore++;
                    break;
                case TileType.Wheat:
                    _wheat++;
                    break;
                case TileType.Wood:
                    _wood++;
                    break;
                case TileType.Wool:
                    _wool++;
                    break;
            }
        }

        public int NumberOfKnights
        {
            get
            {
                int c = 0;
                foreach (AbstractDevelopmentCard card in _playedCards)
                    if (card.Type == DevelopmentCardType.knight)
                        c++;
                return c;
            }
        }

        public int NumberOfVictoryPoints
        {
            get
            {
                int c = 0;
                foreach (AbstractDevelopmentCard card in _playedCards)
                    if (card.Type == DevelopmentCardType.victoryPoint)
                        c++;
                return c;                
            }
        }

        public int NumberOfDevCards
        {
            get
            {
                return _heldCards.Count;
            }
        }


        public int MostRecentlyBuiltSettlementID
        {
            get { return _mostRecentlyBuiltSettlementID; }
            set { _mostRecentlyBuiltSettlementID = value; }
        }

       //public void PayPlayerOner

        public void PayPlayerOneRandomResource(Player player)
        {
            if (TotalNumberOfResources < 1)
                return;

            List<string> availableResourceTypes = new List<string>();
            if (_brick > 0)
                availableResourceTypes.Add("brick");
            if (_ore > 0)
                availableResourceTypes.Add("ore");
            if (_wheat > 0)
                availableResourceTypes.Add("wheat");
            if (_wood > 0)
                availableResourceTypes.Add("wood");
            if (_wool > 0)
                availableResourceTypes.Add("wool");

            string resource = availableResourceTypes[_randomGenerator.Next(0, availableResourceTypes.Count - 1)];

            switch(resource)
            {
                case "brick":
                    _brick--;
                    player.Brick++;
                    break;
                case "ore":
                    _ore--;
                    player.Ore++;
                    break;
                case "wool":
                    _wool--;
                    player.Wool++;
                    break;
                case "wood":
                    _wood--;
                    player.Wood++;
                    break;
                case "wheat":
                    _wheat--;
                    player.Wheat++;
                    break;                
            }
            
        }
            


        public AbstractDevelopmentCard GetDevelopmentCard(string developmentCard)
        {
            DevelopmentCardType type = (DevelopmentCardType)Enum.Parse(typeof(DevelopmentCardType), developmentCard);
            AbstractDevelopmentCard card = _heldCards.Find(delegate(AbstractDevelopmentCard c) { return c.Type == type; });
            if (card != null)
            {
                _heldCards.Remove(card);
                _playedCards.Add(card);
                return card;
            }
            return null;
        }

        public void SetAllDevelopmentCardsPlayable()
        {
            foreach (AbstractDevelopmentCard card in _heldCards)
                card.IsPlayable = true;
        }

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        public int DisplayScore
        {
            get { return _displayScore; }
            set { _displayScore = value; }
        }

        public List<AbstractDevelopmentCard> PlayedCards
        {
            get { return _playedCards; }
            set { _playedCards = value; }
        }

        public List<AbstractDevelopmentCard> HeldCards
        {
            get { return _heldCards; }
            set { _heldCards = value; }
        }

        public List<TradeRecipient> TradeRecipients
        {
            get { return _tradeRecipients; }
        }

        public int TotalNumberOfResources
        {
            get { return _brick + _wood + _ore + _wheat + _wool; }
        }

        public int Wood
        {
            get { return _wood; }
            set { _wood = value; }
        }

        public int Wool
        {
            get { return _wool; }
            set { _wool = value; }
        }

        public int Wheat
        {
            get { return _wheat; }
            set { _wheat = value; }
        }

        public int Brick
        {
            get { return _brick; }
            set { _brick = value; }
        }

        public int Ore
        {
            get { return _ore; }
            set { _ore = value; }
        }

        public int LongestRoadLength
        {
            get { return _longestRoadLength; }
            set 
            {
                if (value < _longestRoadLength)
                    _hadLongestRoadDecreased = true;
                _longestRoadLength = value; 
            }
        }

        public int NumberOfHarbours
        {
            get { return _numberOfHarbours; }
            set { _numberOfHarbours = value; }
        }

        public bool HadLongestRoadLengthDecreased
        {
            get { return _hadLongestRoadDecreased; }
            set { _hadLongestRoadDecreased = value; }
        }

        public int LargestArmySize
        {
            get
            {
                int size = 0;
                foreach (AbstractDevelopmentCard card in _playedCards)
                    if (card.Type == DevelopmentCardType.knight)
                        size++;
                return size;
            }
        }

        public bool HasLargestArmy
        {
            get { return _hasLargestArmy; }
            set { _hasLargestArmy = value; }
        }

        public bool HasLongestRoad
        {
            get { return _hasLongestRoad; }
            set { _hasLongestRoad = value; }
        }

        public bool HasMostHarbours
        {
            get { return _hasMostHarbours; }
            set { _hasMostHarbours = value; }
        }

        public string ID
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Color
        {
            get { return _color; }
        }

        public int PlayerNumber
        {
            get { return _playerNumber; }
        }



    }
}
