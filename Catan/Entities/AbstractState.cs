using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan.Entities
{
    [Serializable]
    public abstract class AbstractState
    {
        protected StateType _stateType;
        protected Board _board;
        protected AbstractState _nextState;
        protected string _currentText;
        protected string _historyText;
        protected string _currentPlayerID;
        protected string _audio;

        public string Audio
        {
            get { return _audio; }
            set { _audio = value; }
        }

        protected List<MapObject> _overlayDetails;

        public List<MapObject> OverlayDetails
        {
            get { return _overlayDetails; }
            set { _overlayDetails = value; }
        }

        public string CurrentPlayerID
        {
            get { return _currentPlayerID; }
        }

        //public abstract AbstractState ProcessInput(Dictionary<string, string> data);

        public abstract AbstractState ProcessInputData(Dictionary<string, string> data);

        public abstract bool IsInputValid(Dictionary<string, string> data, out string message);
        public abstract AbstractState Cancel(Dictionary<string, string> data);

        public AbstractState ProcessInput(Dictionary<string, string> data)
        {
            AbstractState nextState = ProcessInputData(data);
            _board.UpdatePlayerScores();

            return nextState;
        }


        protected AbstractState(Board board, StateType stateType, string currentPlayerID)
        {
            _stateType = stateType;
            _board = board;
            _nextState = null;
            _historyText = string.Empty;
            _currentPlayerID = currentPlayerID;
            _overlayDetails = new List<MapObject>();
            _audio = null;

        }

        public AbstractState NextState
        {
            get { return _nextState; }
            set { _nextState = value; }
        }


        public StateType StateType
        {
            get { return _stateType; }
            set { _stateType = value; }
        }

        public string CurrentText
        {
            get { return _currentText; }
            set { _currentText = value; }
        }

        public string HistoryText
        {
            get { return _historyText; }
            set { _historyText = value; }
        }

        public string StateTypeString
        {
            get { return _stateType.ToString(); }
        }

        public Board Board
        {
            get { return _board; }
        }

            
    }
}
