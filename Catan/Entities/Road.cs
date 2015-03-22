using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities
{
    [Serializable]
    public class Road : MapObject
    {

        private string _playerID;
        public Road(int id, DrawCoordinate drawCoordinate, string image, string playerID)
            : base(id, drawCoordinate, image)
        {
            _playerID = playerID;
        }
        public string PlayerID
        {
            get { return _playerID; }
        }
    }
}
