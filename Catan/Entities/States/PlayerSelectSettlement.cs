using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Interfaces;

namespace Catan.Entities.States
{
    public class PlayerSelectSettlement : AbstractState, IOverlay
    {
        private string _playerID;
        private bool _placeAnywhere;
        private bool _giveResources;


        public PlayerSelectSettlement(string playerID, Board board, bool placeAnywhere, bool giveResources)
            : base(board, Entities.StateType.PLAYER_SELECT_SETTLEMENT, playerID)
        {
            _playerID = playerID;
            _placeAnywhere = placeAnywhere;
            _currentText = board.GetPlayerByID(playerID).Name + ", is building a settlement.";

            _giveResources = giveResources;
            BuildOverlay();
        }

        public void BuildOverlay()
        {
            _overlayDetails.Clear();
            for(int i=0; i<_board.SettlementCoordinates.Count; i++)
            {

                if ((_board.IsTheirAnAdjacentRoad(_playerID, (i + 1).ToString()) || _placeAnywhere) &&
                    !_board.IsTheirAnAdjacentSettlement((i + 1).ToString()))
                {

                    List<Settlement> settlements = _board.Settlements.Where(s => s.ID == (i + 1)).ToList();
                    if (settlements.Count > 0)
                        continue;

                    DrawCoordinate dc = new DrawCoordinate(_board.SettlementCoordinates[i].X, _board.SettlementCoordinates[i].Y, 0);
                    dc.X -= 30;
                    dc.Y -= 28;

                    Player player = _board.GetPlayerByID(_playerID);
                    string clientPredictionImage = "settlement_" + player.Color + ".png";

                    OverlayDetail detail = new OverlayDetail(i + 1, dc, "settlementOverlay.png", _playerID, clientPredictionImage);
                    _overlayDetails.Add(detail);

                }                
                
            }

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
                else if (string.IsNullOrEmpty(data["settlementID"].ToString()) || data["settlementID"].ToString() == "-1")
                {
                    message = "SettlementID appears to be invalid.";
                    return false;
                }
                else if (_board.IsTheirAnAdjacentSettlement(data["settlementID"].ToString()))
                {
                    message = "Invalid settlement location.";
                    return false;
                }
                else if (!_placeAnywhere)
                {
                    if (!_board.IsTheirAnAdjacentRoad(_playerID, data["settlementID"].ToString()))
                    {
                        message = "Must build settlement on a road";
                        return false;
                    }
                }



                foreach (Settlement settlement in _board.Settlements)
                {
                    if (settlement.ID == Int32.Parse(data["settlementID"].ToString()))
                    {
                        message = "A settlement is already established here.";
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

        public override AbstractState Cancel(Dictionary<string, string> data)
        {
            if (_placeAnywhere)
                return this;
            
            Player player = _board.GetPlayerByID(_playerID);

            player.Brick++;
            player.Wood++;
            player.Wheat++;
            player.Wool++;

            return _nextState;
        }

        private void GiveResources(int settlementID)
        {
            foreach (Tile tile in _board.Tiles)
            {
                if (_board.SettlementTileAdjacency[settlementID, tile.ID])
                {
                    Player player = _board.GetPlayerByID(_playerID);
                    player.IncrementPlayerResourceByOne(tile.TileType);
                }
            }
        }



        public override AbstractState ProcessInputData(Dictionary<string, string> data)
        {
            Player player = null;
            string errorMessage = string.Empty;
            int settlementID;
            try
            {
                // validate
                if(!IsInputValid(data, out errorMessage))
                    return this;

                player = _board.GetPlayerByID(_playerID);
                settlementID = Int32.Parse(data["settlementID"].ToString());
                DrawCoordinate drawCoordinate = new DrawCoordinate(_board.SettlementCoordinates[settlementID - 1].X, _board.SettlementCoordinates[settlementID - 1].Y, 0);
                drawCoordinate.X -= 30;
                drawCoordinate.Y -= 28;


                Settlement newSettlement = new Settlement(settlementID, drawCoordinate, "settlement_" + player.Color + ".png", player.ID);
                _board.Settlements.Add(newSettlement);

                player.MostRecentlyBuiltSettlementID = settlementID;

                if (_giveResources)
                    GiveResources(settlementID);
            
                _nextState.HistoryText = player.Name + " built a settlement.";
                _nextState.Audio = Guid.NewGuid().ToString() + ":CONSTRCT.wav";
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
