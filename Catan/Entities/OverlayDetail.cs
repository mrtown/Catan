using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities 
{
    public class OverlayDetail : MapObject
    {
        private string _playerID;
        public OverlayDetail(int id, DrawCoordinate drawCoordinate, string image, string playerID)
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
