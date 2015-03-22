using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities
{
    [Serializable]
    public class BoundingCoordinates 
    {
        private int _ID;
        private DrawCoordinate _coordinate1;
        private DrawCoordinate _coordinate2;
        private DrawCoordinate _coordinate3;
        private DrawCoordinate _coordinate4;

        public BoundingCoordinates(int ID,
                                            DrawCoordinate coordinate1,
                                            DrawCoordinate coordinate2,
                                            DrawCoordinate coordinate3,
                                            DrawCoordinate coordinate4)
        {
            _ID = ID;
            _coordinate1 = coordinate1;
            _coordinate2 = coordinate2;
            _coordinate3 = coordinate3;
            _coordinate4 = coordinate4;
        }

        public int ID
        {
            get { return _ID; }
        }
        public DrawCoordinate Coordinate1
        {
            get { return _coordinate1; }
        }
        public DrawCoordinate Coordinate2
        {
            get { return _coordinate2; }
        }
        public DrawCoordinate Coordinate3
        {
            get { return _coordinate3; }
        }
        public DrawCoordinate Coordinate4
        {
            get { return _coordinate4; }
        }        
    }
}
