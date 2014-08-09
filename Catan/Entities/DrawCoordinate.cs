using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities
{
    public class DrawCoordinate
    {
        private int _x, _y;
        private decimal _drawAngle;
   
        public DrawCoordinate(int x, int y, decimal drawAngle)
        {
            _x = x;
            _y = y;
            _drawAngle = drawAngle;
        }

        public decimal DrawAngle
        {
            get { return _drawAngle; }
            set { _drawAngle = value; }
        }

        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

    }
}
