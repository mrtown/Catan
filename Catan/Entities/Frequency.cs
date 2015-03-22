using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities
{
    [Serializable]
    public class Frequency : MapObject
    {
        private int _frequency;
        private int _id;

        public Frequency(int id, int frequency, DrawCoordinate drawCoordinate)
            : base(id, drawCoordinate, GetImage(frequency))
        {
            _frequency = frequency;
        }

        public int FrequencyValue
        {
            get { return _frequency; }
        }

        public int ID
        {
            get { return _id; }
        }


        private static string GetImage(int frequency)
        {
            return frequency.ToString() + ".png";
        }
    }
}
