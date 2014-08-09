using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Enums;

namespace Catan.Entities
{
    public class Port : MapObject
    {
        private PortType _portType;

        public Port(int id, DrawCoordinate drawCoordinate, PortType portType)
            : base(id, drawCoordinate, GetImageFilename(portType))
        {
            _portType = portType;    
        }

        private static string GetImageFilename(PortType portType)
        {
            switch (portType)
            {
                case Enums.PortType.Brick:
                    return "brickPort.png";
                case Enums.PortType.Ore:
                    return "orePort.png";
                case Enums.PortType.Random:
                    return "randomPort.png";
                case Enums.PortType.Wheat:
                    return "wheatPort.png";
                case Enums.PortType.Wood:
                    return "woodPort.png";
                case Enums.PortType.Wool:
                    return "woolPort.png";
            }

            return null;
        }

        public PortType PortType
        {
            get { return _portType; }
        }
    }

    
}
