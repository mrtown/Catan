using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities.DevelopmentCards
{
    public class VictoryPointCard : AbstractDevelopmentCard
    {
        public VictoryPointCard()
            : base(Enums.DevelopmentCardType.victoryPoint)
        {
        }

        public override AbstractState Play(Player player, Board board)
        {
            return null;                
        }
    }
}
