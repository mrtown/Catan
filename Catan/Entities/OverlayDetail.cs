using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities 
{
    [Serializable]
    public class OverlayDetail : MapObject
    {
        private string _playerID;
        private string _clientPredictionImage;
        public OverlayDetail(int id, DrawCoordinate drawCoordinate, string image, string playerID, string clientPredictionImage)
            : base(id, drawCoordinate, image)
        {
            _playerID = playerID;
            _clientPredictionImage = clientPredictionImage;
        }
        public string PlayerID
        {
            get { return _playerID; }
        }

        public string ClientPredictionImage
        {
            get { return _clientPredictionImage; }
        }
    }
}
