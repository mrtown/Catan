using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Enums;

namespace Catan.Entities
{
    [Serializable]
    public abstract class AbstractDevelopmentCard
    {
        private DevelopmentCardType _type;
        private bool _isPlayable;

        protected AbstractDevelopmentCard(DevelopmentCardType type)
        {
            _type = type;
            _isPlayable = false;
        }

        public abstract AbstractState Play(Player player, Board board);


        public bool IsPlayable
        {
            get { return _isPlayable; }
            set { _isPlayable = value; }
        }

        public DevelopmentCardType Type
        {
            get { return _type; }
        }

        public string TypeString
        {
            get { return _type.ToString(); }
        }
    }

    
}
