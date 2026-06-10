using MatchFactoryCore.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.MatchFactoryCore.Scripts.Data
{
    [Serializable]
    public class DataLevelMatchFactoryNew
    {
        public int Level;
        public float TimeOnLevel;
        public Dictionary<ItemFactoryType, int> TargetItemDictionary;
        public Dictionary<ItemFactoryType, int> OtherItemDictionary;
    }
}
