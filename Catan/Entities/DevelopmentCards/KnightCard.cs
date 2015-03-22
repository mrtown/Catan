using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Entities;
using Catan.Entities.States;


namespace Catan.Entities.DevelopmentCards
{
    [Serializable]
    public class KnightCard : AbstractDevelopmentCard
    {
        public KnightCard()
            : base(Enums.DevelopmentCardType.knight)
        {
        
        }

        public override AbstractState Play(Player player, Board board)
        {
            return new PlayerMoveRobber(player.ID, board);          
        }
    }
}
