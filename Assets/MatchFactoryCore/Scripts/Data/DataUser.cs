using System;
using System.Collections.Generic;

namespace MatchFactoryCore.Scripts.Data
{
    [Serializable]
    public class DataUser
    {
        public int WinStreak;
        public List<ActionType> ActionTypes;
    }
}