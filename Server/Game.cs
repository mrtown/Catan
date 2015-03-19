using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

using Catan.Entities;

namespace Server
{
    
    public class Game
    {        
        private int _numberOfPlayers;
        private List<User> _users;
        private string _id;
        private int _numberOfPlayersConnectedToGame;
        private GameState _gameState;
        Dictionary<string, DateTime> _outstandingPings;
        Dictionary<string, List<Tuple<int, DateTime>>> _pingHistory;
        private static JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private bool _harbourMasterEnabled;

        public User GetUserByID(string ID)
        {
            return _users.Find(delegate(User a) { return a.ID == ID; });
        }

        public Game(int numberOfPlayers, bool harbourMasterEnabled)
        {
            _id = Guid.NewGuid().ToString();
            _numberOfPlayers = numberOfPlayers;
            _users = new List<User>();
            _numberOfPlayersConnectedToGame = 0;
            _gameState = null;
            _outstandingPings = new Dictionary<string, DateTime>();
            _pingHistory = new Dictionary<string, List<Tuple<int, DateTime>>>();
            _harbourMasterEnabled = harbourMasterEnabled;
        }

        #region Ping Stuff
        public void StartPinging()
        {
            Thread t = new Thread(new ParameterizedThreadStart(AsyncSendPings));
            t.Start(_users);            
        }

        public void StartSendingPingStatuses()
        {
            Thread t = new Thread(new ParameterizedThreadStart(AsyncSendPingStatuses));
            t.Start(null);                    
        }

        private void AsyncSendPingStatuses(object parameters)
        {
            while (true)
            {
                Thread.Sleep(1000);
                Dictionary<string, double> pingStatuses = new Dictionary<string, double>();

                foreach(User user in _users)
                {
                    if (_pingHistory.ContainsKey(user.ID) && _pingHistory[user.ID].Count > 0)                        
                        pingStatuses.Add(user.Name, Math.Round(_pingHistory[user.ID].Average(t => t.Item1)));    
                }
       
                for (int i = 0; i < _pingHistory.Count; i++)
                {
                    var list = _pingHistory.ElementAt(i).Value.Where(x => (DateTime.Now - x.Item2).Seconds < 10).ToList();
                    _pingHistory[_pingHistory.ElementAt(i).Key] = list;//new KeyValuePair<string, List<Tuple<int, DateTime>>>(_pingHistory.ElementAt(i).Key, list);
                }

                ProtocolMessage msg = new ProtocolMessage(Message.PING_STATUSES.ToString(), pingStatuses);

                string json = _serializer.Serialize(msg);
  
                foreach (User gameUser in _users)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(MessageBroker.AsyncProcess));
                    KeyValuePair<User, string> details = new KeyValuePair<User, string>(gameUser, json);
                    t.Start(details);
                }   
                
            }
        }

        private void AsyncSendPings(object usrs)
        {
            List<User> users = (List<User>)usrs;
            
            while (true)
            {
                Thread.Sleep(1000);
                foreach (User user in users)
                {
                    string pingID = Guid.NewGuid().ToString();

                    ProtocolMessage msg = new ProtocolMessage(Message.PING.ToString(), pingID);

                    string json = _serializer.Serialize(msg);

                    _outstandingPings.Add(pingID, DateTime.Now);
                    user.Context.Send(json);
                }
            }
        }

        public void ProcessReceivedPing(string playerID, string pingID)
        {
            if (_outstandingPings.Keys.Contains(pingID))
            {
                TimeSpan diff = (DateTime.Now - _outstandingPings[pingID]);
               
                AddToPingHistory(playerID, diff, _outstandingPings[pingID]);
                _outstandingPings.Remove(pingID);

                //System.Diagnostics.Debug.Print(
                //    diff.Milliseconds.ToString()
                //);

            }
        }

        private void AddToPingHistory(string playerID, TimeSpan diff, DateTime pingTimestamp)
        {
            if (!_pingHistory.ContainsKey(playerID))
            {
                List<Tuple<int, DateTime>> listOfDetails = new List<Tuple<int, DateTime>>();
                Tuple<int, DateTime> details = new Tuple<int, DateTime>(diff.Milliseconds, pingTimestamp);
                listOfDetails.Add(details);
                _pingHistory.Add(playerID, listOfDetails);
            }
            else
            {
                _pingHistory[playerID].Add(new Tuple<int, DateTime>(diff.Milliseconds, pingTimestamp));
            }
        }
        #endregion

        public bool IsHarbourMasterEnabled
        {
            get { return _harbourMasterEnabled; }
        }

        public GameState GameState
        {
            get { return _gameState; }
            set { _gameState = value; }
        }

        public int NumberOfPlayersConnectedToGame
        {
            get { return _numberOfPlayersConnectedToGame; }
            set { _numberOfPlayersConnectedToGame = value; }
        }

        public string ID
        {
            get { return _id; }
        }

        public int NumberOfPlayers
        {
            get { return _numberOfPlayers; }
            set { _numberOfPlayers = value; }
        }

        public List<User> Users
        {
            get { return _users; }
            set { _users = value; }
        }
    }
}
