using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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



        public User GetUserByID(string ID)
        {
            return _users.Find(delegate(User a) { return a.ID == ID; });
        }

        public Game(int numberOfPlayers)
        {
            _id = Guid.NewGuid().ToString();
            _numberOfPlayers = numberOfPlayers;
            _users = new List<User>();
            _numberOfPlayersConnectedToGame = 0;
            _gameState = null;
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
