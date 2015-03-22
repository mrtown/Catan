﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Interfaces;

namespace Catan.Entities.States
{
    [Serializable]
    public class PlayerSelectRoad : AbstractState, IOverlay
    {
        private string _playerID;
        private bool _cameFromDevelomentCard;
        private bool _placeAnywhere;

        public PlayerSelectRoad(string playerID, Board board, bool placeAnywhere, bool cameFromDevelopmentCard)
            : base(board, Entities.StateType.PLAYER_SELECT_ROAD, playerID)
        {            
            _playerID = playerID;
            _currentText = board.GetPlayerByID(playerID).Name + ", is building a road.";
            _cameFromDevelomentCard = cameFromDevelopmentCard;
            _placeAnywhere = placeAnywhere;
            //BuildOverlay();
        }



            

        public string PlayerID
        {
            get { return _playerID; }
        }

        public override bool IsInputValid(Dictionary<string, string> data, out string message)
        {
            try
            {
                if (_playerID != data["playerID"].ToString())
                {
                    message = "A player is trying to establish a settlement when it is not their turn.";
                    return false;
                }
                else if (string.IsNullOrEmpty(data["roadID"].ToString()) || data["roadID"].ToString() == "-1")
                {
                    message = "RoadID appears to be invalid.";
                    return false;
                }

                if (!_board.IsTheirAnAdjacentSettlementOrRoad(_playerID, data["roadID"].ToString()))
                {
                    message = "Invalid road location";
                    return false;
                }

                if (!_placeAnywhere)
                {
                    if (!IsRoadNextToMostRecentSettlement(data["roadID"].ToString()))
                    {
                        message = "Road must be attached to most recent settlement";
                        return false;
                    }
                }

                foreach (Road road in _board.Roads)
                {
                    if (road.ID == Int32.Parse(data["roadID"].ToString()))
                    {
                        message = "A road is already established here.";
                        return false;
                    }
                }

            }
            catch (Exception e)
            {
                message = e.Message;
                return false;
            }

            message = "success";
            return true;
        }


        public bool IsRoadNextToMostRecentSettlement(string roadID)
        {
            Player player = _board.GetPlayerByID(_playerID);
            return _board.IsRoadNextToSettlement(Int32.Parse(roadID), player.MostRecentlyBuiltSettlementID);
        }

        public override AbstractState Cancel(Dictionary<string, string> data)
        {
            Player player = _board.GetPlayerByID(_playerID);

            if (_placeAnywhere)
            {
                if (_cameFromDevelomentCard)
                {
                    player.AddRoadBuildCardBackToHand();
                    return _nextState;
                }
                else
                {                   
                    player.Brick++;
                    player.Wood++;
                    return _nextState;
                }
            }
            else
            {
                return this;
            }


        }

        public void BuildOverlay()
        {
            string roadImageIndex = "";
            _overlayDetails.Clear();
            for (int i = 0; i < _board.RoadCoordinates.Count; i++)
            {

                if ((_board.IsTheirAnAdjacentSettlementOrRoad(_playerID, (i + 1).ToString())) &&
                     (IsRoadNextToMostRecentSettlement((i + 1).ToString()) || _placeAnywhere))
                {

                    List<Road> roads = _board.Roads.Where(r => r.ID == (i + 1)).ToList();
                    if (roads.Count > 0)
                        continue;

                    DrawCoordinate drawCoordinate = new DrawCoordinate(_board.RoadCoordinates[i].X, _board.RoadCoordinates[i].Y, 0);
                    if (Board.Image1RoadIndexes.Contains(i + 1))
                    {
                        roadImageIndex = "_1";
                        drawCoordinate.X -= 29;
                        drawCoordinate.Y -= 7;
                    }
                    else if (Board.Image2RoadIndexes.Contains(i + 1))
                    {
                        roadImageIndex = "_2";
                        drawCoordinate.X -= 20;
                        drawCoordinate.Y -= 26;
                    }
                    else if (Board.Image3RoadIndexes.Contains(i + 1))
                    {
                        roadImageIndex = "_3";
                        drawCoordinate.X -= 20;
                        drawCoordinate.Y -= 26;
                    }

                    Player player = _board.GetPlayerByID(_playerID);
                    string clientPredictionImage = "road_" + player.Color + roadImageIndex + ".png";

                    OverlayDetail detail = new OverlayDetail(i + 1, drawCoordinate, "roadOverlay" + roadImageIndex + ".png", _playerID, clientPredictionImage);
                    _overlayDetails.Add(detail);

                }

            }

        }

        public override AbstractState ProcessInputData(Dictionary<string, string> data)
        {
            Player player = null;
            string errorMessage = string.Empty;
            int roadID;
            try
            {
                // validate
                if(!IsInputValid(data, out errorMessage))
                    return this;

                player = _board.GetPlayerByID(_playerID);
                roadID = Int32.Parse(data["roadID"].ToString());
                DrawCoordinate drawCoordinate = new DrawCoordinate(_board.RoadCoordinates[roadID - 1].X, _board.RoadCoordinates[roadID - 1].Y, 0);


                string roadImageIndex = string.Empty;
                if (Board.Image1RoadIndexes.Contains(roadID))
                {
                    roadImageIndex = "_1";
                    drawCoordinate.X -= 29;
                    drawCoordinate.Y -= 7;
                }
                else if (Board.Image2RoadIndexes.Contains(roadID))
                {
                    roadImageIndex = "_2";
                    drawCoordinate.X -= 20;
                    drawCoordinate.Y -= 26;
                }
                else if (Board.Image3RoadIndexes.Contains(roadID))
                {
                    roadImageIndex = "_3";
                    drawCoordinate.X -= 20;
                    drawCoordinate.Y -= 26;
                }

                Road newRoad = new Road(roadID, drawCoordinate, "road_" + player.Color + roadImageIndex + ".png", player.ID);
              
                _board.Roads.Add(newRoad);

                _nextState.HistoryText = player.Name + " built a road.";
                _nextState.Audio = Guid.NewGuid().ToString() + ":MINE.wav";
            }
            catch (Exception e)
            {
            
            }

            if (_nextState != null)
            {
                return _nextState;
            }
            
             return this;
        }        
    }
}
