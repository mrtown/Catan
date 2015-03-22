using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web.Script.Serialization;

using Alchemy;
using Alchemy.Classes;

using Catan.Entities;


namespace Server
{
    public class Server
    {
        private static List<User> _lobbyUsers = new List<User>();
        private static Object _lock = new Object();
        public static List<Context> AllConnections = null;
        private static List<Game> _games = new List<Game>();
        private static List<DrawCoordinate> _settlementCoordinates;
        private static List<DrawCoordinate> _roadCoordinates;
        private static List<DrawCoordinate> _tileCoordinates;
        private static List<DrawCoordinate> _playerCoordinates;
        private static List<DrawCoordinate> _portCoordinates;
        private static bool[,] _portSettlementAdjacency;
        private static bool[,] _settlementAdjacency;
        private static bool[,] _roadAdjacency;
        private static bool[,] _settlementRoadAdjacency;
        private static bool[,] _settlementTileAdjacency;
        private static List<int> _image1RoadIndexes;
        private static List<int> _image2RoadIndexes;
        private static List<int> _image3RoadIndexes;

        public static Game GetGameByID(string ID)
        {
            return _games.Find(delegate(Game a) { return a.ID == ID; });
        }

        public static User GetLobbyUserByID(string ID)
        {
            return Server._lobbyUsers.Find(delegate(User a) { return a.ID == ID; });
        }

        public static List<int> Image1RoadIndexes
        {
            get { return _image1RoadIndexes; }
        }

        public static List<int> Image2RoadIndexes
        {
            get { return _image2RoadIndexes; }
        }

        public static List<int> Image3RoadIndexes
        {
            get { return _image3RoadIndexes; }
        }

        public static void RemoveLobbyUser(string clientAddress)
        {
            lock (_lock)
            {
                User existingUser = null;
                existingUser = _lobbyUsers.Find(delegate(User u) { return u.ClientAddress == clientAddress; });
                if (existingUser != null)
                    _lobbyUsers.Remove(existingUser);
            }            
        }

        public static void AddLobbyUser(User user) 
        {
            lock(_lock)
            {
                RemoveLobbyUser(user.ClientAddress);
                _lobbyUsers.Add(user);
            }
        }

        public static void ClearLobbyUsers()
        {
            lock(_lock)
            {
                _lobbyUsers.Clear();
            }
        }

        public static List<User> GetLobbyUsers()
        {
            return _lobbyUsers;
        }


        public static List<Game> Games
        {
            get { return _games; }
            set { _games = value; }
        }

        private static void ParseCoordinates()
        {
            _portCoordinates = FileSystemDataParser.ParseCoordinateData("C:\\projects\\AlchemyServer\\PortCoordinates.txt");
            _playerCoordinates = FileSystemDataParser.ParseCoordinateData("C:\\projects\\AlchemyServer\\PlayerCoordinates.txt");
            _tileCoordinates = FileSystemDataParser.ParseCoordinateData("C:\\projects\\AlchemyServer\\TileCoordinates.txt");
            _settlementCoordinates = FileSystemDataParser.ParseCoordinateData("C:\\projects\\AlchemyServer\\SettlementCoordinates.txt");            
            _roadCoordinates = FileSystemDataParser.ParseCoordinateData("C:\\projects\\AlchemyServer\\RoadCoordinates.txt");
            _settlementTileAdjacency = FileSystemDataParser.ParseAdjacentData("C:\\projects\\AlchemyServer\\SettlementTileAdjacency.txt");
            _roadAdjacency = FileSystemDataParser.ParseAdjacentData("C:\\projects\\AlchemyServer\\RoadAdjacency.txt");
            _portSettlementAdjacency = FileSystemDataParser.ParseAdjacentData("C:\\projects\\AlchemyServer\\PortSettlementAdjaceny.txt");           
            _settlementAdjacency = FileSystemDataParser.ParseAdjacentData("C:\\projects\\AlchemyServer\\SettlementAdjacency.txt");
            _settlementRoadAdjacency = FileSystemDataParser.ParseAdjacentData("C:\\projects\\AlchemyServer\\SettlementRoadAdjacency.txt");
            _image1RoadIndexes = FileSystemDataParser.ParseRoadImageIndexes("C:\\projects\\AlchemyServer\\image1RoadIndexes.txt");
            _image2RoadIndexes = FileSystemDataParser.ParseRoadImageIndexes("C:\\projects\\AlchemyServer\\image2RoadIndexes.txt");
            _image3RoadIndexes = FileSystemDataParser.ParseRoadImageIndexes("C:\\projects\\AlchemyServer\\image3RoadIndexes.txt");
        }

        public static List<DrawCoordinate> PlayerCoordinates
        {
            get { return _playerCoordinates; }
        }

        public static List<DrawCoordinate> TileCoordinates
        {
            get { return _tileCoordinates; }
        }

        public static List<DrawCoordinate> PortCoordinates
        {
            get { return _portCoordinates; }
        }

        public static bool[,] SettlementTileAdjacency
        {
            get { return _settlementTileAdjacency; }
        }

        public static List<DrawCoordinate> RoadCoordinates
        {
            get { return _roadCoordinates; }
        }

        public static List<DrawCoordinate> SettlementCoordinates
        {
            get { return _settlementCoordinates; }
        }

        public static bool[,] RoadAdjacency
        {
            get { return _roadAdjacency; }
        }
        public static bool[,] SettlementAdjacency
        {
            get { return _settlementAdjacency; }
        }
        public static bool[,] SettlementRoadAdjacency
        {
            get { return _settlementRoadAdjacency; }
        }

        public static bool[,] PortSettlementAdjacency
        {
            get { return _portSettlementAdjacency; }
        }

        static void Main(string[] args)
        {
            Console.Write("Enter port: ");
            string data = Console.ReadLine();
            int port;

            if (Int32.TryParse(data, out port))
            {
                ParseCoordinates();

                var aServer = new WebSocketServer(port, IPAddress.Any)
                {
                    OnReceive = OnReceive,
                    //OnSend = OnSend,
                    //OnConnect = OnConnect,
                    OnConnected = OnConnected,
                    OnDisconnect = OnDisconnect,
                    TimeOut = new TimeSpan(0, 5, 0)
                };

                if (AllConnections == null)
                    AllConnections = aServer.AllConnections;

                aServer.Start();
            }
            else
            {
                Console.WriteLine("The port was unacceptable.");
                Console.ReadKey();
            }
        }

        static void OnDisconnect(UserContext context)
        {
            RemoveLobbyUser(context.ClientAddress.ToString());
            MessageBroker.BroadcastLobbyList();
            Console.WriteLine("Client Disconnect From : " +
            context.ClientAddress.ToString());            
        }


        static void OnReceive(UserContext context)
        {
            try
            {
                MessageBroker.ProcessMessage(context);
            }
            catch (Exception ex)
            {
                // TODO: log the issue
            }
            
        }

        static void OnConnected(UserContext context)
        {
            Console.WriteLine("Client Connection From : " +
            context.ClientAddress.ToString());
        }  
    }
}
