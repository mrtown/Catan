using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities
{
    [Serializable]
    public class Board
    {
        private List<Port> _ports;
        private List<Tile> _tiles;
        private List<Road> _roads;
        private List<Settlement> _settlements;
        private GameState _gameState;
        private List<Enums.TileType> _deck;
        private bool _isHarbourMasterEnabled;
        int _frequencyIndex = 0;
        private List<Frequency> _frequencies = CreateFrequencyDeck();
        private List<Catan.Enums.PortType> _portDeck = CreatePortDeck();

        private static object _lock = new Object();

        private static Random _randomNumberGenerator = new Random(DateTime.Now.Millisecond);

        public Board(GameState gameState, bool isHarbourMasterEnabled)
        {
            _isHarbourMasterEnabled = isHarbourMasterEnabled;
            _ports = new List<Port>();
            _tiles = new List<Tile>();
            _roads = new List<Road>();
            _settlements = new List<Settlement>();
            _deck = CreateTileDeck();
            _gameState = gameState;
            CreateTiles();
            CreatePorts();

            if (true)
                RandomizeFrequencies();
        }

        private void RandomizeFrequencies()
        {
            Random randomGenerator = new Random(DateTime.Now.Millisecond);

            if (_tiles != null && _tiles.Count > 0)
            {
                List<Frequency> randomizedFrequencies;
                List<Frequency> frequencies = _tiles.Where(t => t.TileType != Enums.TileType.Dessert).Select(t => t.Frequency).ToList();
                _tiles.ForEach(t => t.Frequency = null);

                do
                {
                    randomizedFrequencies = frequencies.OrderBy(f => Guid.NewGuid().ToString()).ToList();
                    
                    foreach(Tile tile in _tiles)
                    {
                        if (tile.TileType == Enums.TileType.Dessert)
                            continue;
                        
                        Frequency frequency = randomizedFrequencies.Take(1).First();
                        randomizedFrequencies.RemoveAt(0);
                        tile.Frequency = frequency;
                    }

                } while (!IsFrequencyDistributionValid());

                foreach (Tile tile in _tiles)
                    if (tile.TileType != Enums.TileType.Dessert)
                        tile.SetDrawCoordinate();
            }
        }

        private bool IsFrequencyDistributionValid()
        {
            List<int> settlementIds = new List<int>();
            for(int k=0;k < _gameState.SettlementTileAdjacency.GetLength(0);k++)
                for (int l = 0; l < _gameState.SettlementTileAdjacency.GetLength(1); l++)
                {
                    if (_gameState.SettlementTileAdjacency[k, l])
                        settlementIds.Add(k);
                }

            settlementIds = settlementIds.Distinct().ToList();

            foreach (int settlementId in settlementIds)
            {
                List<int> adjacentTileIds = new List<int>();
                for(int i=1; i<=_tiles.Count; i++)
                {
                    if (_gameState.SettlementTileAdjacency[settlementId, i])
                        adjacentTileIds.Add(i);
                }

                int numberOfHighYieldTiles = 0;
                foreach (int tileId in adjacentTileIds)
                {
                    
                    Tile tile = _tiles.Where(t => t.ID == tileId).First();
                    if (tile.TileType != Enums.TileType.Dessert)
                        if (tile.Frequency.FrequencyValue == 8 ||
                            tile.Frequency.FrequencyValue == 6)
                            numberOfHighYieldTiles += 1;

                    if (numberOfHighYieldTiles > 1)
                        return false;
                }

                //foreach (Tile tileA in _tiles.Where(t => adjacentTileIds.Contains(t.ID)))
                //{
                //    if (tileA.TileType == Enums.TileType.Dessert)
                //        continue;

                //    foreach (Tile tileB in _tiles.Where(t => adjacentTileIds.Contains(t.ID)))
                //    {
                //        if (tileB.TileType == Enums.TileType.Dessert)
                //            continue;

                //        if (tileA.ID == tileB.ID)
                //            continue;

                //        if (_gameState.SettlementAdjacency[tileA.ID, tileB.ID])
                //            if ((tileA.Frequency.FrequencyValue == 6 || tileA.Frequency.FrequencyValue == 8) &&
                //                (tileB.Frequency.FrequencyValue == 6 || tileB.Frequency.FrequencyValue == 8))
                //                return false;
                //    }
                //}

            }

            return true;
        }

        public int Rolled
        {
            get { return _gameState.Rolled; }
            set { _gameState.Rolled = value; }
        }


        public AbstractDevelopmentCard GetRandomDevelopmentCard()
        {
            lock (_lock)
            {
                AbstractDevelopmentCard card;

                if (_gameState.DevelopmentCardDeck.Count < 1)
                    return null;

                int index = _randomNumberGenerator.Next(1, _gameState.DevelopmentCardDeck.Count + 1);
                card = _gameState.DevelopmentCardDeck[index - 1];
                _gameState.DevelopmentCardDeck.RemoveAt(index - 1);
                return card;
            }
        }

        public string Message
        {
            get { return _gameState.Message; }
            set { _gameState.Message = value; }
        }

        public bool PlayerHasAGeneralPort(string playerID)
        {
            foreach (Port port in _ports)
            {
                if (port.PortType == Enums.PortType.Random)
                    foreach (Settlement settlement in _settlements)
                        if (settlement.PlayerID == playerID)
                            if (_gameState.PortSettlementAdjacency[port.ID, settlement.ID])
                                return true;
            }
            return false;
        }

        private Enums.PortType GetType(string resource)
        {
            switch (resource)
            {
                case "wood":
                    return Enums.PortType.Wood;
                case "ore":
                    return Enums.PortType.Ore;
                case "wheat":
                    return Enums.PortType.Wheat;
                case "brick":
                    return Enums.PortType.Brick;
                case "wool":
                    return Enums.PortType.Wool;                        
            }
            return Enums.PortType.Unknown;
        }

        public bool PlayerHasPortOfType(string playerID, string resource)
        {
            foreach (Port port in _ports)
            {
                if (port.PortType == GetType(resource))
                    foreach (Settlement settlement in _settlements)
                        if (settlement.PlayerID == playerID)
                            if (_gameState.PortSettlementAdjacency[port.ID, settlement.ID])
                                return true;
            }
            return false;

        }

        public void UpdateRobber(int tileID)
        {
            Tile currentRobber = _tiles.Find(delegate(Tile t) { return t.Robber; });
            currentRobber.Robber = false;

            _tiles.Find(delegate(Tile t) { return t.ID == tileID; }).Robber = true;
        }

        public void UpdatePlayerScores()
        {
            _gameState.PlayerWithLongestRoad = WhoHasLongestRoad();
            _gameState.PlayerWithLargestArmy = WhoHasLargestArmy();
            
            if (_isHarbourMasterEnabled)
                _gameState.PlayerWithMostHarbours = WhoHasMostHarbours();

            _gameState.Players.ForEach(p => CalculatePlayerScore(p));

            CheckWinningCondition();
            
        }

        private void CheckWinningCondition()
        {
            int winScore = _isHarbourMasterEnabled ? 11 : 10;

            Player player = _gameState.Players.Where(p => p.Score >= winScore).FirstOrDefault();
            if (player != null)
                _gameState.Winner = player.Name;
        }

        private void CalculatePlayerScore(Player p)
        {
            int score = 0;
            
            if (p.HasLargestArmy)
                score += 2;
            if (p.HasLongestRoad)
                score += 2;                      
            if (p.HasMostHarbours && _isHarbourMasterEnabled)
                score += 2;
            
            List<Settlement> settlements = _settlements.Where(s => s.PlayerID == p.ID).ToList();

            score += settlements.Count();
            score += settlements.Where(s => s.IsCity).Count();

            score += p.HeldCards.Where(c => c.Type == Enums.DevelopmentCardType.victoryPoint).Count();

            p.Score = score;

            // update display score
            p.DisplayScore = p.Score;
            p.DisplayScore -= p.HeldCards.Where(c => c.Type == Enums.DevelopmentCardType.victoryPoint).Count();
        }

        private Player WhoHasLargestArmy()
        {
            Player largestArmyPlayer = null;
            if (_gameState.PlayerWithLargestArmy == null)
            {
                foreach (Player player in _gameState.Players)
                {
                    if (player.LargestArmySize == 3)
                    {
                        largestArmyPlayer = player;
                        player.HasLargestArmy = true;
                        break;
                    }
                }
            }
            else
            {
                int currentLargestArmySize = _gameState.PlayerWithLargestArmy.LargestArmySize;
                foreach (Player player in _gameState.Players)
                {
                    if (player.LargestArmySize > currentLargestArmySize)
                    {
                        largestArmyPlayer = player;
                        player.HasLargestArmy = true;
                        _gameState.PlayerWithLargestArmy.HasLargestArmy = false;
                        break;
                    }
                    else
                    {
                        largestArmyPlayer = _gameState.PlayerWithLargestArmy;
                    }
                }
            }

            return largestArmyPlayer;
        }

        private int CalculateNumberOfHarbours(Player player)
        {
            int harbours = 0;

            foreach (Settlement s in _settlements)
                if (s.PlayerID == player.ID)
                    foreach (Port p in _ports)
                        if (_gameState.PortSettlementAdjacency[p.ID, s.ID])
                            harbours += s.IsCity ? 2 : 1;
                         
            return harbours;
        }

        private Player WhoHasMostHarbours()
        {

            Player mostHarboursPlayer = null;
            foreach (Player player in _gameState.Players)
                player.NumberOfHarbours = CalculateNumberOfHarbours(player);

            int maxHarbourCount = _gameState.Players.Max(p => p.NumberOfHarbours);
            List<Player> playersWithMostHarbours = _gameState.Players.Where(p => p.NumberOfHarbours == maxHarbourCount &&
                                                                                 p.NumberOfHarbours >= 3).ToList();

            if (_gameState.PlayerWithMostHarbours == null)
            {
                if (playersWithMostHarbours.Count == 0)
                    return null;
                else if (playersWithMostHarbours.Count == 1)
                {
                    mostHarboursPlayer = playersWithMostHarbours[0];
                    playersWithMostHarbours[0].HasMostHarbours = true;
                }
            }
            else
            {
                if (playersWithMostHarbours.Count == 1)
                {
                    mostHarboursPlayer = playersWithMostHarbours[0];
                    playersWithMostHarbours[0].HasMostHarbours = true;
                    if (playersWithMostHarbours[0].ID != _gameState.PlayerWithMostHarbours.ID)
                        _gameState.PlayerWithMostHarbours.HasMostHarbours = false;
                }
                else
                {
                    mostHarboursPlayer = _gameState.PlayerWithMostHarbours;
                }
             }

            return mostHarboursPlayer;            
        }

        private Player WhoHasLongestRoad()
        {
            Player longestRoadPlayer = null;
            foreach (Player player in _gameState.Players)
            {
                List<Road> roads = _roads.Where(r => r.PlayerID == player.ID).ToList();
                player.LongestRoadLength = CalculateMaxRoadLength(roads);
            }

            int maxRoadLength = _gameState.Players.Max(p => p.LongestRoadLength);
            List<Player> playersWithMaxRoadLength = _gameState.Players.Where(p => p.LongestRoadLength == maxRoadLength &&
                                                                                  p.LongestRoadLength >= 5).ToList();
            if (_gameState.PlayerWithLongestRoad == null)
            {

                if (playersWithMaxRoadLength.Count == 0)
                {
                    return null;
                }
                else if (playersWithMaxRoadLength.Count == 1)
                {
                    longestRoadPlayer = playersWithMaxRoadLength[0];
                    playersWithMaxRoadLength[0].HasLongestRoad = true;                    
                }


                //foreach (Player player in _gameState.Players)
                //{                    
                //    if (player.LongestRoadLength == 5)
                //    {
                //        longestRoadPlayer = player;
                //        player.HasLongestRoad = true;
                //        break;
                //    }
                //}
            }
            else
            {
                if (playersWithMaxRoadLength.Count == 1)
                {
                    longestRoadPlayer = playersWithMaxRoadLength[0];
                    playersWithMaxRoadLength[0].HasLongestRoad = true;
                    if (playersWithMaxRoadLength[0].ID != _gameState.PlayerWithLongestRoad.ID)
                        _gameState.PlayerWithLongestRoad.HasLongestRoad = false;
                }
                else if (playersWithMaxRoadLength.Count > 1)
                {
                    if (_gameState.Players.Exists(p => p.HadLongestRoadLengthDecreased && p.LongestRoadLength == maxRoadLength))
                    {
                        _gameState.PlayerWithLongestRoad.HasLongestRoad = false;
                    }
                    else
                    {
                        longestRoadPlayer = _gameState.PlayerWithLongestRoad;
                    }
                }

                //int currentLongestRoadLength = _gameState.PlayerWithLongestRoad.LongestRoadLength;
                //foreach (Player player in _gameState.Players)
                //{
                //    if (player.LongestRoadLength > currentLongestRoadLength)
                //    {
                //        longestRoadPlayer = player;
                //        player.HasLongestRoad = true;
                //        _gameState.PlayerWithLongestRoad.HasLongestRoad = false;
                //        break;
                //    }
                //    else
                //    {
                //        longestRoadPlayer = _gameState.PlayerWithLongestRoad;
                //    }
                //}
            }

            _gameState.Players.ForEach(p => p.HadLongestRoadLengthDecreased = false);

            return longestRoadPlayer;
        }

        private int CalculateMaxRoadLength(List<Road> roads)
        {
            int maxRoadLength = 0;
            
            // for each road apply the longest road algo
            foreach (Road road in roads)
            {
                // start algo
                Stack<LongestRoadTraceState> stack = new Stack<LongestRoadTraceState>();
                LongestRoadTraceState startState = new LongestRoadTraceState(road, roads);
                stack.Push(startState);

                while (stack.Count > 0)
                {
                    LongestRoadTraceState state = stack.Pop();
                    
                    // count the current road
                    state.RoadEnabledStatus[state.Road] = false;
                    state.Count++;

                    // how many "enabled" adjacent roads?
                    List<Road> adjacentEnabledRoads = GetAdjacentEnabledRoads(state);

                    if (adjacentEnabledRoads.Count == 0)
                        maxRoadLength = Math.Max(state.Count, maxRoadLength);
                    else if (adjacentEnabledRoads.Count == 1)
                    {
                        state.Road = adjacentEnabledRoads[0];
                        stack.Push(state);
                    }
                    else if (adjacentEnabledRoads.Count > 1)
                    {
                        foreach (Road r in adjacentEnabledRoads)
                        {
                            LongestRoadTraceState intermediateState = state.DeepCopy();
                            DisableAllAdjacentEnabledRoadsExcept(intermediateState, r);
                            intermediateState.Road = r;
                            stack.Push(intermediateState);                        
                        }
                    }
                    else
                        throw new Exception("Invalid number of adjacent roads found when calculating longest road");
                }
            }

            return maxRoadLength;
        }

        private void DisableAllAdjacentEnabledRoadsExcept(LongestRoadTraceState state, Road r)
        {
            foreach (Road rd in state.AllRoads)
                if (state.RoadEnabledStatus[rd])
                    if (_gameState.RoadAdjacency[state.Road.ID, rd.ID])
                        if (r.ID != rd.ID)
                            state.RoadEnabledStatus[rd] = false;
        }

        //private Road GetFirstAdjacentEnabledRoad(LongestRoadTraceState state)
        //{
        //    foreach (Road r in state.AllRoads)
        //        if (state.RoadEnabledStatus[r])
        //            if (_gameState.RoadAdjacency[state.Road.ID, r.ID])
        //                return r;

        //    return null;
        //}

        private List<Road> GetAdjacentEnabledRoads(LongestRoadTraceState state)
        {
            List<Road> adjacentEnabledRoads = new List<Road>();
            foreach (Road r in state.AllRoads)
                if (state.RoadEnabledStatus[r])
                    if (_gameState.RoadAdjacency[state.Road.ID, r.ID])
                        if (!AreRoadsBlocked(state.Road.ID, r.ID))
                            adjacentEnabledRoads.Add(r);

            return adjacentEnabledRoads;
        }

        private bool AreRoadsBlocked(int roadA, int roadB)
        {
            string playerID = _roads.Where(r => r.ID == roadA).First().PlayerID;

            List<Settlement> settlements = Settlements.Where(s => s.PlayerID != playerID).ToList();
            foreach (Settlement s in settlements)
                if (_gameState.SettlementRoadAdjacency[s.ID, roadA])
                    if (_gameState.SettlementRoadAdjacency[s.ID, roadB])
                        return true;
            return false;
        }

        public List<Settlement> Settlements
        {
            get { return _settlements; }
            set { _settlements = value; }
        }

        public bool[,] SettlementTileAdjacency
        {
            get { return _gameState.SettlementTileAdjacency; }
        }

        public List<AbstractDevelopmentCard> DevelopmentCardDeck
        {
            get { return _gameState.DevelopmentCardDeck; }
            set { _gameState.DevelopmentCardDeck = value; }
        }

        public Player GetNextPlayer(string currentPlayerID)
        {
            Player currentPlayer = GetPlayerByID(currentPlayerID);
            int index = _gameState.Players.IndexOf(currentPlayer);

            if ((index + 1) == _gameState.Players.Count)
                return _gameState.Players[0];
            else
                return _gameState.Players[index + 1];
        }

        public Player GetPlayerByNumber(int selectedPlayer)
        {
            foreach (Player p in Players)
                if (p.PlayerNumber == selectedPlayer)
                    return p;
            return null;
        }

        public Player GetPlayerByID(string ID)
        {
            return _gameState.GetPlayerByID(ID);
        }

        public List<Player> Players
        {
            get { return _gameState.Players; }
        }

        public bool IsTheirAnAdjacentRoad(string playerID, string settlementID)
        {
            foreach (Road road in _roads)
            {
                if (road.PlayerID == playerID)
                {
                    if (_gameState.SettlementRoadAdjacency[Int32.Parse(settlementID), road.ID])
                        return true;
                }
            }
            return false;
        }

        public bool IsRoadNextToSettlement(int roadID, int settlementID)
        {
            if (_gameState.SettlementRoadAdjacency[settlementID, roadID])
                return true;

            return false;            
        }

        public bool IsTheirAnAdjacentSettlementOrRoad(string playerID, string roadID)
        {
            foreach (Settlement settlement in _settlements)
            {
                if (settlement.PlayerID == playerID)
                {
                    if (_gameState.SettlementRoadAdjacency[settlement.ID, Int32.Parse(roadID)])
                        return true;
                }
            }

            foreach (Road road in _roads)
            {
                if (road.PlayerID == playerID)
                {
                    if (_gameState.RoadAdjacency[road.ID, Int32.Parse(roadID)])
                        if (!AreRoadsBlocked(road.ID, Int32.Parse(roadID)))
                            return true;
                }
            }

            return false;    
        }

        public bool IsTheirAnAdjacentSettlement(string settlementID)
        {
            if (_settlements.Count == 0)
                return false;
            else
            {
                foreach (Settlement settlement in _settlements)
                {
                    if (_gameState.SettlementAdjacency[Int32.Parse(settlementID), settlement.ID])
                        return true;
                }
            }

            return false;
        }

        public List<DrawCoordinate> RoadCoordinates
        {
            get { return _gameState.RoadCoordinates; }
        }

        public List<Port> Ports
        {
            get { return _ports; }
        }

        public List<DrawCoordinate> SettlementCoordinates
        {
            get { return _gameState.SettlementCoordinates; }
        }

        public List<int> Image1RoadIndexes
        {
            get { return _gameState.Image1RoadIndexes; }
        }
        public List<int> Image2RoadIndexes
        {
            get { return _gameState.Image2RoadIndexes; }
        }
        public List<int> Image3RoadIndexes
        {
            get { return _gameState.Image3RoadIndexes; }
        }

        //public GameState GameState
        //{
        //    get { return _gameState; }
        //}

        private  void CreatePorts()
        {
            
            for(int i=0; i<_gameState.PortCoordinates.Count; i++)
            {
                Port newPort = new Port(i + 1, _gameState.PortCoordinates[i], GetRandomPortType());
                _ports.Add(newPort);
            }            
        }

        private static List<Catan.Enums.PortType> CreatePortDeck()
        {
            List<Catan.Enums.PortType> list = new List<Enums.PortType>();

            list.Add(Enums.PortType.Random);
            list.Add(Enums.PortType.Random);
            list.Add(Enums.PortType.Random);
            list.Add(Enums.PortType.Random);
            list.Add(Enums.PortType.Brick);
            list.Add(Enums.PortType.Ore);
            list.Add(Enums.PortType.Wheat);
            list.Add(Enums.PortType.Wood);
            list.Add(Enums.PortType.Wool);

            return list;
        }

        private Catan.Enums.PortType GetRandomPortType()
        {
            Enums.PortType returnValue;
            int randomIndex = _randomNumberGenerator.Next(1, _portDeck.Count + 1);

            returnValue = _portDeck[randomIndex - 1];

            _portDeck.RemoveAt(randomIndex - 1);

            return returnValue;
        }

        private static List<Frequency> CreateFrequencyDeck()
        {

            List<Frequency> frequencies = new List<Frequency>();

            frequencies.Add(new Frequency(1, 5, null));
            frequencies.Add(new Frequency(2, 2, null));
            frequencies.Add(new Frequency(3, 6, null));
            frequencies.Add(new Frequency(4, 3, null));
            frequencies.Add(new Frequency(5, 8, null));
            frequencies.Add(new Frequency(6, 10, null));
            frequencies.Add(new Frequency(7, 9, null));
            frequencies.Add(new Frequency(8, 12, null));
            frequencies.Add(new Frequency(9, 11, null));
            frequencies.Add(new Frequency(10, 4, null));
            frequencies.Add(new Frequency(11, 8, null));
            frequencies.Add(new Frequency(12, 10, null));
            frequencies.Add(new Frequency(13, 9, null));
            frequencies.Add(new Frequency(14, 4, null));
            frequencies.Add(new Frequency(15, 5, null));
            frequencies.Add(new Frequency(16, 6, null));
            frequencies.Add(new Frequency(17, 3, null));
            frequencies.Add(new Frequency(18, 11, null));

            return frequencies;


        }

        private List<Enums.TileType> CreateTileDeck()
        {
            List<Enums.TileType> deck = new List<Enums.TileType>();

            deck.Add(Enums.TileType.Brick);
            deck.Add(Enums.TileType.Brick);
            deck.Add(Enums.TileType.Brick);
            deck.Add(Enums.TileType.Ore);
            deck.Add(Enums.TileType.Ore);
            deck.Add(Enums.TileType.Ore);
            deck.Add(Enums.TileType.Dessert);

            deck.Add(Enums.TileType.Wheat);
            deck.Add(Enums.TileType.Wheat);
            deck.Add(Enums.TileType.Wheat);
            deck.Add(Enums.TileType.Wheat);

            deck.Add(Enums.TileType.Wood);
            deck.Add(Enums.TileType.Wood);
            deck.Add(Enums.TileType.Wood);
            deck.Add(Enums.TileType.Wood);

            deck.Add(Enums.TileType.Wool);
            deck.Add(Enums.TileType.Wool);
            deck.Add(Enums.TileType.Wool);
            deck.Add(Enums.TileType.Wool);

            return deck;
        }

        private Enums.TileType GetRandomTileFromDeck()
        {
            Enums.TileType returnValue;
            int randomIndex = _randomNumberGenerator.Next(1, _deck.Count+1);

            returnValue = _deck[randomIndex - 1];

            _deck.RemoveAt(randomIndex - 1);

            return returnValue;
        }

        private void AddTile(int id, DrawCoordinate drawCoordinate)
        {
            Tile tile = null;
            Enums.TileType tileType = GetRandomTileFromDeck();
            if (tileType != Enums.TileType.Dessert)
            {
                tile = new Tile(id, drawCoordinate, tileType, _frequencies[_frequencyIndex], _gameState.TileCoordinates);
                _frequencyIndex++;
            }
            else
            {
                tile = new Tile(id, drawCoordinate, tileType, _gameState.TileCoordinates);
                tile.Robber = true;
            }

            _tiles.Add(tile);
        }

        public void UpdateRobberDrawCoordinates()
        {
            foreach (Tile tile in _tiles)
            {
                tile.SetRobberDrawCoordinate();
            }
        }



        private void CreateTiles()
        {
            DrawCoordinate drawCoordinate = null;

            drawCoordinate = new DrawCoordinate(500, 50, 0);
            AddTile(1, drawCoordinate);

            drawCoordinate = new DrawCoordinate(350, 138, 0);
            AddTile(2, drawCoordinate);

            drawCoordinate = new DrawCoordinate(200, 226, 0);
            AddTile(4, drawCoordinate);

            drawCoordinate = new DrawCoordinate(200, 400, 0);
            AddTile(9, drawCoordinate);

            drawCoordinate = new DrawCoordinate(200, 574, 0);
            AddTile(14, drawCoordinate);

            drawCoordinate = new DrawCoordinate(350, 660, 0);
            AddTile(17, drawCoordinate);

            drawCoordinate = new DrawCoordinate(500, 748, 0);
            AddTile(19, drawCoordinate);

            drawCoordinate = new DrawCoordinate(650, 660, 0);
            AddTile(18, drawCoordinate);

            drawCoordinate = new DrawCoordinate(800, 574, 0);
            AddTile(16, drawCoordinate);

            drawCoordinate = new DrawCoordinate(800, 400, 0);
            AddTile(11, drawCoordinate);

            drawCoordinate = new DrawCoordinate(800, 226, 0);
            AddTile(6, drawCoordinate);

            drawCoordinate = new DrawCoordinate(650, 138, 0);
            AddTile(3, drawCoordinate);

            drawCoordinate = new DrawCoordinate(500, 226, 0);
            AddTile(5, drawCoordinate);

            drawCoordinate = new DrawCoordinate(350, 312, 0);
            AddTile(7, drawCoordinate);

            drawCoordinate = new DrawCoordinate(350, 486, 0);
            AddTile(12, drawCoordinate);

            drawCoordinate = new DrawCoordinate(500, 574, 0);
            AddTile(15, drawCoordinate);

            drawCoordinate = new DrawCoordinate(650, 486, 0);
            AddTile(13, drawCoordinate);

            drawCoordinate = new DrawCoordinate(650, 312, 0);
            AddTile(8, drawCoordinate);

            drawCoordinate = new DrawCoordinate(500, 400, 0);
            AddTile(10, drawCoordinate);

            UpdateRobberDrawCoordinates();

        }

        public List<Road> Roads
        {
            get { return _roads; }
            set { _roads = value; }
        }

        public List<Tile> Tiles
        {
            get { return _tiles; }
            set { _tiles = value; }
        }
    }
}
