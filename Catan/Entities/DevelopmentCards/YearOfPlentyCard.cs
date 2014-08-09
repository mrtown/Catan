using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Entities.States;

namespace Catan.Entities.DevelopmentCards
{
    public class YearOfPlentyCard : AbstractDevelopmentCard
    {
        public YearOfPlentyCard()
            : base(Enums.DevelopmentCardType.yearOfPlenty)
        {
        
        }

        public override AbstractState Play(Player player, Board board)
        {               
            return new PlayerSelectTwoResourcesFromBank(player.ID, board);
        }
    }
}
