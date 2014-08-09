using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alchemy.Classes;

namespace Server
{
    public class User
    {
        private string _name;
        private string _id;
        private UserContext _context;
        private bool _isInLobby;
        private string _clientAddress;

        public User(string name, UserContext context)
        {
            _name = name;
            _context = context;
            _id = Guid.NewGuid().ToString();
            _isInLobby = true;
            _clientAddress = context.ClientAddress.ToString();
        }

        public string ClientAddress
        {
            get { return _clientAddress; }
        }

        public bool IsInLobby
        {
            get { return _isInLobby; }
            set { _isInLobby = value; }
        }

        public UserContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string ID
        {
            get { return _id; }
        }
    }

}
