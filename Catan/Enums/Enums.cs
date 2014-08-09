using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Enums
{
    public enum TileType
    {
        Brick = 1,
        Wood = 2,
        Wool = 3, 
        Ore = 4, 
        Wheat = 5,
        Dessert = 6
    };

    public enum DevelopmentCardType
    {
        knight = 1,
        victoryPoint = 2,
        roadBuilding = 3,
        monopoly = 4,
        yearOfPlenty = 5
    }

    public enum PortType
    {
        Brick = 1,
        Wood = 2, 
        Ore = 3,
        Wheat = 4, 
        Wool = 5,
        Random = 6,
        Unknown = 7
    }
}
