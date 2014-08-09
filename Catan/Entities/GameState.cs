using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Entities.States;
using Catan.Entities.DevelopmentCards;

namespace Catan.Entities
{

    public class GameState
    {
        private Board _board;
        private AbstractState _state;
        private List<DrawCoordinate> _portCoordinates;
        private List<DrawCoordinate> _playerCoordinates;
        private List<DrawCoordinate> _settlementCoordinates;
        private List<DrawCoordinate> _roadCoordinates;
        private List<DrawCoordinate> _tileCoordinates;
        private bool[,] _portSettlementAdjacency;
        private bool[,] _settlementAdjacency;
        private bool[,] _roadAdjacency;
        private bool[,] _settlementRoadAdjacency;
        private bool[,] _settlementTileAdjacency;
        private List<Player> _players;
        private List<int> _image1RoadIndexes;
        private List<int> _image2RoadIndexes;
        private List<int> _image3RoadIndexes;
        private List<AbstractDevelopmentCard> _developmentCardDeck;
        private Player _playerWithLongestRoad;
        private Player _playerWithLargestArmy;
        private int _rolled = 0;
        private string _message;

        private string _winner;

        public string Winner
        {
            get { return _winner; }
            set { _winner = value; }
        }

        public Player PlayerWithLongestRoad
        {
            get { return _playerWithLongestRoad; }
            set { _playerWithLongestRoad = value; }
        }

        public Player PlayerWithLargestArmy
        {
            get { return _playerWithLargestArmy; }
            set { _playerWithLargestArmy = value; }
        }

        public int Rolled
        {
            get { return _rolled; }
            set { _rolled = value; }
        }

        private List<Player> BuildPlayers(List<KeyValuePair<string, string>> users)
        {
            int currentPlayer = 1;
            string currentColor = null;
            List<Player> players = new List<Player>();

            users = users.OrderBy(x => Guid.NewGuid()).ToList();

            foreach (KeyValuePair<string, string> user in users)
            {
                if (currentPlayer == 1)
                    currentColor = "green";
                else if (currentPlayer == 2)
                    currentColor = "yellow";
                else if (currentPlayer == 3)
                    currentColor = "blue";
                else if (currentPlayer == 4)
                    currentColor = "red";

                Player newPlayer = new Player(user.Key, user.Value, currentColor, currentPlayer, BuildTradeRecipientList(user, users));
                players.Add(newPlayer);
                currentPlayer++;
            }
            
            return players;
        }

        private List<TradeRecipient> BuildTradeRecipientList(KeyValuePair<string, string> user, List<KeyValuePair<string,string>> users)
        {
            List<TradeRecipient> recipients = new List<TradeRecipient>();

            foreach (KeyValuePair<string, string> u in users)
            {
                if (u.Key != user.Key)
                    recipients.Add(new TradeRecipient(u.Key, u.Value));
            }

            recipients.Add(new TradeRecipient("bank", "Bank"));
            recipients.Add(new TradeRecipient("robber", "Robber"));

            return recipients;
        }

        public Player GetPlayerByID(string playerID)
        {
            return _players.Find(delegate(Player player){ return player.ID == playerID; });
        }

        private AbstractState CreateStateFlow()
        {
            AbstractState ptr = null;
            AbstractState start = null;

            for (int i = 0; i < _players.Count; i++)
            {
                AbstractState state = new PlayerSelectSettlement(_players[i].ID, _board, true, false);
                state.NextState = new PlayerSelectRoad(_players[i].ID, _board, false, false);

                if (ptr == null)
                {
                    start = state;
                    ptr = state.NextState;
                }
                else
                {
                    ptr.NextState = state;
                    ptr = state.NextState;
                }
            }

            for (int i = _players.Count-1; i > -1; i--)
            {
                AbstractState state = new PlayerSelectSettlement(_players[i].ID, _board, true, true);
                state.NextState = new PlayerSelectRoad(_players[i].ID, _board, false, false);

                if (ptr == null)
                    ptr = state.NextState;
                else
                {
                    ptr.NextState = state;
                    ptr = state.NextState;
                }
            }

            ptr.NextState = new PlayerTurn(_players[0].ID, _board);

            return start;
        }

        public GameState(List<KeyValuePair<string, string>> users, 
                         List<DrawCoordinate> portCoordinates,
                         List<DrawCoordinate> playerCoordinates,
                         List<DrawCoordinate> settlementCoordinates, 
                         List<DrawCoordinate> roadCoordinates, 
                         List<DrawCoordinate> tileCoordinates,
                         bool[,] portSettlementAdjacency,
                         bool[,] settlementAdjacency, 
                         bool[,] roadAdjacency, 
                         bool[,] settlementRoadAdjacency,
                         bool[,] settlementTileAdjacency,
                         List<int> imageRoadIndexes1,
                         List<int> imageRoadIndexes2,
                         List<int> imageRoadIndexes3)

        {
            
            _players = BuildPlayers(users);

            _portCoordinates = portCoordinates;
            _developmentCardDeck = CreateDevelopmentCardDeck();
            _playerCoordinates = playerCoordinates;
            _roadCoordinates = roadCoordinates;
            _settlementCoordinates = settlementCoordinates;
            _tileCoordinates = tileCoordinates;
            _settlementAdjacency = settlementAdjacency;
            _portSettlementAdjacency = portSettlementAdjacency;
            _roadAdjacency = roadAdjacency;
            _settlementRoadAdjacency = settlementRoadAdjacency;
            _settlementTileAdjacency = settlementTileAdjacency;

            _image1RoadIndexes = imageRoadIndexes1;
            _image2RoadIndexes = imageRoadIndexes2;
            _image3RoadIndexes = imageRoadIndexes3;

            _playerWithLargestArmy = null;
            _playerWithLongestRoad = null;

            _winner = null;

            _message = null;

            _board = new Board(this);
            _state = CreateStateFlow();
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public List<AbstractDevelopmentCard> DevelopmentCardDeck
        {
            get { return _developmentCardDeck; }
            set { _developmentCardDeck = value; }
        }

        private List<AbstractDevelopmentCard> CreateDevelopmentCardDeck()
        {
            List<AbstractDevelopmentCard> deck = new List<AbstractDevelopmentCard>();

            //14-- knights
            for (int i = 0; i < 14; i++)
                deck.Add(new KnightCard());
            
            //5 VPs
            for (int i = 0; i < 5; i++)
                deck.Add(new VictoryPointCard());

            //2 road building
            for (int i = 0; i < 2; i++)
                deck.Add(new RoadBuildingCard());

            //2 monopoly
            for (int i = 0; i < 2; i++)
                deck.Add(new MonopolyCard());

            //2 year of plenty
            for (int i = 0; i < 2; i++)
                deck.Add(new YearOfPlentyCard());

            return deck;
        }


        public List<int> Image1RoadIndexes
        {
            get { return _image1RoadIndexes; }
        }

        public List<int> Image2RoadIndexes
        {
            get { return _image2RoadIndexes; }
        }

        public List<int> Image3RoadIndexes
        {
            get { return _image3RoadIndexes; }
        }

        private void BuildImageRoadIndexes()
        {
            


        }

        public List<Player> Players
        {
            get { return _players; }
        }

        public AbstractState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public Board Board
        {
            get { return _board; }
            set { _board = value; }
        }

        public List<DrawCoordinate> SettlementCoordinates
        {
            get { return _settlementCoordinates; }
            set { _settlementCoordinates = value; }
        }

        public List<DrawCoordinate> PlayerCoordinates
        {
            get { return _playerCoordinates; }
            set { _playerCoordinates = value; }
        }

        public List<DrawCoordinate> RoadCoordinates
        {
            get { return _roadCoordinates; }
            set { _roadCoordinates = value; }
        }

        public List<DrawCoordinate> TileCoordinates
        {
            get { return _tileCoordinates; }
            set { _tileCoordinates = value; }
        }

        public List<DrawCoordinate> PortCoordinates
        {
            get { return _portCoordinates; }
        }

        public bool[,] SettlementTileAdjacency
        {
            get { return _settlementTileAdjacency; }
            set { _settlementTileAdjacency = value; }
        }

        public bool[,] SettlementAdjacency
        {
            get { return _settlementAdjacency; }
        }

        public bool[,] PortSettlementAdjacency
        {
            get { return _portSettlementAdjacency; }
        }

        public bool[,] RoadAdjacency
        {
            get { return _roadAdjacency; }
        }
        public bool[,] SettlementRoadAdjacency
        {
            get { return _settlementRoadAdjacency; }
        }
    }



    public enum StateType
    {
        PLAYER_SELECT_SETTLEMENT = 1,
        PLAYER_SELECT_ROAD = 2,
        PLAYER_TURN = 3,
        PLAYER_ROLL = 4,
        PLAYER_PASS = 5, 
        PLAYER_BUILD = 6,
        PLAYER_SELECT_CITY = 7,
        PLAYER_PLAY_CARD = 8, 
        PLAYER_MOVE_ROBBER= 9,
        PLAYER_TRADE = 10,
        TRADE_CONFIRM = 11,
        PLAYER_SELECT_TWO_RESOURCES_FROM_BANK = 12,
        PLAYER_SELECT_RESOURCE_FOR_MONOPOLY = 13,
        PLAYER_PAY_ROBBER = 14,
        PLAYER_SELECT_PLAYER = 15
    }





}
