using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Enums;

namespace Catan.Entities
{
    public class Tile : MapObject
    {
        private TileType _tileType;
        private Frequency _frequency;
        private List<DrawCoordinate> _tileCoordinates;
        private bool _robber;
        private DrawCoordinate _robberDrawDetails;
        
        //public Tile(int id, DrawCoordinate drawCoordinate, TileType tileType) 
        //    : base(id, drawCoordinate)
        //{
        //    _tileType = tileType;
        //}

        private static string GetImageFilename(TileType type)
        {
            if (type == Enums.TileType.Brick)
                return "brickTile.png";
            else if (type == Enums.TileType.Dessert)
                return "dessertTile.png";
            else if (type == Enums.TileType.Ore)
                return "oreTile.png";
            else if (type == Enums.TileType.Wheat)
                return "wheatTile.png";
            else if (type == Enums.TileType.Wood)
                return "woodTile.png";
            else if (type == Enums.TileType.Wool)
                return "woolTile.png";

            return string.Empty;
        }

        public Tile(int id, DrawCoordinate drawCoordinate, TileType tileType, Frequency frequency, List<DrawCoordinate> tileCoordinates)
            : base(id, drawCoordinate, GetImageFilename(tileType))
        {
            _tileType = tileType;
            _frequency = frequency;
            _tileCoordinates = tileCoordinates;
            SetDrawCoordinate();
            //SetRobberDrawCoordinate();
            _robber = false;
            _robberDrawDetails = null;
        }

        public Tile(int id, DrawCoordinate drawCoordinate, TileType tileType, List<DrawCoordinate> tileCoordinates)
            : base(id, drawCoordinate, GetImageFilename(tileType))
        {
            _id = id;
            _tileCoordinates = tileCoordinates;
            _drawCoordinate = drawCoordinate;
            _tileType = tileType;
            //SetRobberDrawCoordinate();
            _robber = false;
        }

        public void SetDrawCoordinate()
        {
            DrawCoordinate dc = new DrawCoordinate(_tileCoordinates[ID - 1].X-44, _tileCoordinates[ID - 1].Y-40, 0);
            _frequency.DrawCoordinate = dc;
        }

        public DrawCoordinate RobberDrawDetails
        {
            get { return _robberDrawDetails; }
            set { _robberDrawDetails = value; }
        }

        public void SetRobberDrawCoordinate()
        {
            DrawCoordinate dc = new DrawCoordinate(_tileCoordinates[ID - 1].X - 40, _tileCoordinates[ID - 1].Y - 56, 0);
            _robberDrawDetails = dc;
        }

        public bool Robber
        {
            get { return _robber; }
            set { _robber = value; }
        }

        public Frequency Frequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }
        

        public TileType TileType
        {
            get { return _tileType; }
            set { _tileType = value; }                 
        }
    }
}
