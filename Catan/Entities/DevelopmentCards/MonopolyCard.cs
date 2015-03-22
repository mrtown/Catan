using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Entities.States;

namespace Catan.Entities.DevelopmentCards
{
    [Serializable]
    public class MonopolyCard : AbstractDevelopmentCard
    {
        public MonopolyCard()
            : base(Enums.DevelopmentCardType.monopoly)
        {
        }


        public override AbstractState Play(Player player, Board board)
        {
            return new PlayerSelectResourceForMonopoly(player.ID, board);
        }
    }
}
