using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities
{
    [Serializable]
    public abstract class MapObject
    {
        protected int _id;
        protected string _image;
        protected DrawCoordinate _drawCoordinate;


        public MapObject(int id, DrawCoordinate drawCoordinate)
        {
            _id = id;
            _drawCoordinate = drawCoordinate;
            _image = null;
        }

        public MapObject(int id, DrawCoordinate drawCoordinate, string image)
        {
            _id = id;
            _drawCoordinate = drawCoordinate;
            _image = image;
        }

        public string Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public DrawCoordinate DrawCoordinate
        {
            get { return _drawCoordinate; }
            set { _drawCoordinate = value; }
        }
    }
}
