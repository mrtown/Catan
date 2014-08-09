using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities
{
    public class Settlement : MapObject
    {
        private string _playerID;
        private bool _isCity;

        public Settlement(int id, DrawCoordinate drawCoordinate, string image, string playerID)
            : base(id, drawCoordinate, image)
        {
            _playerID = playerID;
            _isCity = false;
        }

        public void UpgradeToCity(Player player)
        {
            _isCity = true;
            _image = "city_" + player.Color + ".png";            
        }

        public bool IsCity
        {
            get { return _isCity; }
        }

        public string PlayerID
        {
            get { return _playerID; }
        }
    }
}
