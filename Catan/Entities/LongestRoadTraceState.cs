using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities
{
    [Serializable]
    public class LongestRoadTraceState
    {
        private Road _road;
        private int _count;
        private Dictionary<Road, bool> _roadEnabledStatus;
        private List<Road> _allRoads;

        private LongestRoadTraceState()
        {
        }

        public LongestRoadTraceState(Road road, List<Road> allRoads)
        {
            _count = 0;
            _roadEnabledStatus = new Dictionary<Road, bool>();
            _road = road;
            _allRoads = allRoads;

            foreach (Road r in _allRoads)
                _roadEnabledStatus.Add(r, true);
        }

        public LongestRoadTraceState(Road road, int count, Dictionary<Road, bool> roadEnabledStatus, List<Road> allRoads)
        {
            _count = count;
            _roadEnabledStatus = roadEnabledStatus;
            _road = road;        
        }

        public Road Road { get { return _road; } set { _road = value; } }
        public int Count { get { return _count; } set { _count = value; } }
        public Dictionary<Road, bool> RoadEnabledStatus { get { return _roadEnabledStatus; } set { _roadEnabledStatus = value; } }
        public List<Road> AllRoads { get { return _allRoads; } set { _allRoads = value; } }


        public LongestRoadTraceState DeepCopy()
        {
            LongestRoadTraceState newState = new LongestRoadTraceState();
            newState._count = Count;
            newState._road = Road;
            newState._allRoads = AllRoads;

            newState._roadEnabledStatus = new Dictionary<Road, bool>();

            foreach (KeyValuePair<Road, bool> status in RoadEnabledStatus)
                newState._roadEnabledStatus.Add(status.Key, status.Value);

            return newState;
        }
    }
}
