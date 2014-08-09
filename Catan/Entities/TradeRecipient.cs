using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities
{
    public class TradeRecipient
    {
        private string _id;
        private string _name;

        public TradeRecipient(string id, string name)
        {
            _id = id;
            _name = name;
        }

        public string ID
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
