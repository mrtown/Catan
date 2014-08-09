using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Entities.States;

namespace Catan.Entities.DevelopmentCards
{
    public class RoadBuildingCard : AbstractDevelopmentCard
    {
        public RoadBuildingCard()
            : base(Enums.DevelopmentCardType.roadBuilding)
        {
        }

        public override AbstractState Play(Player player, Board board)
        {
            int c = 0;
            int numberOfAllowedRoadsToBuild = 0;
            foreach (Road road in board.Roads)
                if (road.PlayerID == player.ID)
                    c++;

            if (c >= 15)
                numberOfAllowedRoadsToBuild = 0;
            else
                numberOfAllowedRoadsToBuild = 15 - c;

            if (numberOfAllowedRoadsToBuild == 0)
                return null;
            else if (numberOfAllowedRoadsToBuild == 1)
            {
                return new PlayerSelectRoad(player.ID, board, true, true);
            }
            else
            {
                PlayerSelectRoad newRoad1 = new PlayerSelectRoad(player.ID, board, true, true);
                PlayerSelectRoad newRoad2 = new PlayerSelectRoad(player.ID, board, true, true);
                newRoad1.NextState = newRoad2;
                return newRoad1;            
            }
        }
    }
}
