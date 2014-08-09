using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Web.Script.Serialization;

using Alchemy.Classes;
using Catan.Entities;

namespace Server
{
    public class ProtocolMessage
    {
        public string Control;
        public object Payload;

        public ProtocolMessage(string control, object payload)
        {
            this.Control = control;
            this.Payload = payload;
        }

    }

    public class MessageBroker
    {
        private static JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private static Object _lock = new Object();

        public static void ProcessMessage(UserContext context) 
        {
            string data = context.DataFrame.ToString();
            string[] parsedMessage = data.Split(':');

            switch ((Message)Enum.Parse(typeof(Message), parsedMessage[0]))
            {
                case Message.NEW_LOBBY_USER:
                    ProcessNewLobbyUser(parsedMessage[1], context);
                    break;
                case Message.CHAT_MESSAGE:
                    ProcessChatMessage(parsedMessage[1], parsedMessage[2]);
                    break;
                case Message.START_GAME:
                    StartGame();
                    break;
                case Message.ENTERING_GAME:
                    lock(_lock) 
                        EnteringGame(parsedMessage[1], parsedMessage[2], context);
                    break;
                case Message.PLAYER_SELECT_SETTLEMENT:
                    ProcessPlayerSelectSettlement(parsedMessage[1], parsedMessage[2], parsedMessage[3]);
                    break;
                case Message.PLAYER_SELECT_ROAD:
                    ProcessPlayerSelectRoad(parsedMessage[1], parsedMessage[2], parsedMessage[3]);
                    break;
                case Message.PLAYER_ROLL:
                    ProcessPlayerRoll(parsedMessage[1], parsedMessage[2]);
                    break;
                case Message.PLAYER_PASS:
                    ProcessPlayerPass(parsedMessage[1], parsedMessage[2]);
                    break;
                case Message.PLAYER_BUILD:
                    ProcessPlayerBuild(parsedMessage[1], parsedMessage[2], parsedMessage[3]);
                    break;
                case Message.PLAYER_SELECT_CITY:
                    ProcessPlayerSelectCity(parsedMessage[1], parsedMessage[2], parsedMessage[3]);
                    break;
                case Message.PLAYER_PLAY_CARD:
                    ProcessPlayerPlayCard(parsedMessage[1], parsedMessage[2], parsedMessage[3]);
                    break;
                case Message.PLAYER_MOVE_ROBBER:
                    ProcessPlayerMoveRobber(parsedMessage[1], parsedMessage[2], parsedMessage[3]);
                    break;
                case Message.TRADE_CONFIRM:
                    ProcessTradeConfirm(parsedMessage[1], parsedMessage[2], parsedMessage[3]);
                    break;
                case Message.PLAYER_SELECT_RESOURCE_FOR_MONOPOLY:
                    ProcessPlayerSelectResourceForMonopoly(parsedMessage[1], parsedMessage[2], parsedMessage[3]);
                    break;
                case Message.PLAYER_SELECT_PLAYER:
                    ProcessPlayerSelectPlayer(parsedMessage[1], parsedMessage[2], parsedMessage[3]);
                    break;
                case Message.PLAYER_CANCEL:
                    ProcessPlayerCancel(parsedMessage[1], parsedMessage[2]);
                    break;
                case Message.PLAYER_TRADE:
                    lock (_lock)
                    {
                        ProcessPlayerTrade(parsedMessage[1],
                                           parsedMessage[2],
                                           parsedMessage[3],
                                           parsedMessage[4],
                                           parsedMessage[5],
                                           parsedMessage[6],
                                           parsedMessage[7],
                                           parsedMessage[8]);
                    }
                    break;


                        
            }
            
        }

        private static void ProcessPlayerCancel(string gameID, string playerID)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            
            data.Add("playerID", playerID);

            game.GameState.State = game.GameState.State.Cancel(data);

            Multicast(game);

        }

        private static void ProcessPlayerSelectPlayer(string gameID, string playerID, string selectedPlayer)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("selectedPlayer", selectedPlayer);

            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);    
        }

        private static void ProcessPlayerSelectResourceForMonopoly(string gameID, string playerID, string resource)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("resource", resource);

            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game); 
        }

        private static void ProcessTradeConfirm(string gameID, string recipientPlayerID, string confirm)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("recipientPlayerID", recipientPlayerID);
            data.Add("confirm", confirm);
              
            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);            
        }

        private static void ProcessPlayerTrade(string gameID, string playerID, string recipientID, string brickAmount, string oreAmount, string wheatAmount, string woodAmount, string woolAmount)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("action", Message.PLAYER_TRADE.ToString());
            data.Add("recipientID", recipientID);
            data.Add("brickAmount", brickAmount);
            data.Add("oreAmount", oreAmount);
            data.Add("wheatAmount", wheatAmount);
            data.Add("woodAmount", woodAmount);
            data.Add("woolAmount", woolAmount);
            
            

            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);       
        }

        private static void ProcessPlayerMoveRobber(string gameID, string playerID, string tileID)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("tileID", tileID);

            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);         
        }

        private static void ProcessPlayerPlayCard(string gameID, string playerID, string card)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("action", Message.PLAYER_PLAY_CARD.ToString());
            data.Add("card", card);

            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);                     
        }

        private static void ProcessPlayerBuild(string gameID, string playerID, string buildItem)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("action", Message.PLAYER_BUILD.ToString());
            data.Add("buildItem", buildItem);

            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);            
        }

        private static void ProcessPlayerPass(string gameID, string playerID)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("action", Message.PLAYER_PASS.ToString());

            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);           
        }

        private static void ProcessPlayerRoll(string gameID, string playerID)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("action", Message.PLAYER_ROLL.ToString());

            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);
        }

        private static void Multicast(Game game)
        {
            ProtocolMessage msg = new ProtocolMessage(Message.GAME_STATE.ToString(), game.GameState);
            string json = _serializer.Serialize(msg);
            foreach (User gameUser in game.Users)
            {
                Thread t = new Thread(new ParameterizedThreadStart(AsyncProcess));
                KeyValuePair<User, string> details = new KeyValuePair<User, string>(gameUser, json);
                t.Start(details);
            }    
        }

        private static void AsyncProcess(object details)
        {
            KeyValuePair<User, string> data = (KeyValuePair<User, string>)details;
            data.Key.Context.Send(data.Value);            
        }

        private static void ProcessPlayerSelectCity(string gameID, string playerID, string settlementID)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("settlementID", settlementID);

            // key update
            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);     
        }

        private static void ProcessPlayerSelectRoad(string gameID, string playerID, string roadID)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("roadID", roadID);

            // key update
            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);      
        }
        
        private static void ProcessPlayerSelectSettlement(string gameID, string playerID, string settlementID)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            game.GameState.Message = null;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("playerID", playerID);
            data.Add("settlementID", settlementID);

            // key update
            game.GameState.State = game.GameState.State.ProcessInput(data);

            Multicast(game);
        }

        private static void EnteringGame(string userID, string gameID, UserContext context)
        {
            Game game = Server.GetGameByID(gameID);
            if (game == null)
                return;

            User user = game.GetUserByID(userID);
            if (user == null)
                return;

            game.NumberOfPlayersConnectedToGame++;                        
            user.Context = context;

            if (game.NumberOfPlayersConnectedToGame == 1)
            {
                List<KeyValuePair<string, string>> newUsers = new List<KeyValuePair<string, string>>();
                foreach (User u in game.Users)
                {
                    KeyValuePair<string, string> newUser = new KeyValuePair<string, string>(u.ID, u.Name);
                    newUsers.Add(newUser);
                }

                game.GameState = new GameState(newUsers, 
                                               Server.PortCoordinates,
                                               Server.PlayerCoordinates,
                                               Server.SettlementCoordinates, 
                                               Server.RoadCoordinates, 
                                               Server.TileCoordinates,
                                               Server.PortSettlementAdjacency,
                                               Server.SettlementAdjacency,
                                               Server.RoadAdjacency,
                                               Server.SettlementRoadAdjacency,
                                               Server.SettlementTileAdjacency,
                                               Server.Image1RoadIndexes,
                                               Server.Image2RoadIndexes,
                                               Server.Image3RoadIndexes);
            }

            Multicast(game);

        }

        private static void StartGame()
        {
            List<User> lobbyUsers = Server.GetLobbyUsers();
            Game newGame = new Game(lobbyUsers.Count);
            Server.Games.Add(newGame);            
            newGame.Users.AddRange(lobbyUsers);
            Server.ClearLobbyUsers();

            ProtocolMessage msg = new ProtocolMessage(Message.START_GAME.ToString(), newGame.ID);
            foreach (User user in newGame.Users)
            {
                string json = _serializer.Serialize(msg);
                user.Context.Send(json);
            }                
        }

        private static void ProcessChatMessage(string userID, string message)
        {
            User user = Server.GetLobbyUserByID(userID);

            if (user == null)
                return;

            ProtocolMessage msg = new ProtocolMessage(Message.CHAT_MESSAGE.ToString(), user.Name + ": " + message);
            foreach (User lobbyUser in Server.GetLobbyUsers())
            {
                string json = _serializer.Serialize(msg);
                lobbyUser.Context.Send(json);
            }
        }

        public static void BroadcastLobbyList()
        {
            string lobbyList = string.Empty;
            foreach (User user in Server.GetLobbyUsers())
            {
                lobbyList += user.Name + "\n";
            }

            ProtocolMessage msg = new ProtocolMessage(Message.LOBBY_LIST.ToString(), lobbyList);

            foreach (User user in Server.GetLobbyUsers())
            {
                string json = _serializer.Serialize(msg);
                user.Context.Send(json);
            }            

            //foreach (Context context in Server.AllConnections)
            //{
            //    JavaScriptSerializer serializer = new JavaScriptSerializer();
            //    string json = serializer.Serialize(msg);
            //    context.UserContext.Send(json);
            //}
        }

        private static void ProcessNewLobbyUser(string data, UserContext context)
        {
            User user = new User(data, context);            
            Server.AddLobbyUser(user);

            // send new user id back to client
            ProtocolMessage msg = new ProtocolMessage(Message.NEW_LOBBY_USER.ToString(), user.ID);
            string json = _serializer.Serialize(msg);
            context.Send(json);

            // send list of users
            BroadcastLobbyList();

        }
    }

    




    public enum Message 
    {
        NEW_LOBBY_USER = 1, 
        CHAT_MESSAGE = 2,
        LOBBY_LIST = 3,
        START_GAME = 4,
        ENTERING_GAME = 5,
        GAME_STATE = 6,
        PLAYER_SELECT_SETTLEMENT = 7,
        PLAYER_SELECT_ROAD = 8,
        PLAYER_TURN = 9, 
        PLAYER_ROLL = 10,
        PLAYER_PASS = 11,
        PLAYER_BUILD = 12,
        PLAYER_SELECT_CITY = 13,
        PLAYER_PLAY_CARD = 14,
        PLAYER_MOVE_ROBBER = 15,
        PLAYER_TRADE = 16,
        TRADE_CONFIRM = 17,
        PLAYER_SELECT_RESOURCE_FOR_MONOPOLY = 18,
        PLAYER_SELECT_PLAYER = 19,
        PLAYER_CANCEL = 20
    }
}
