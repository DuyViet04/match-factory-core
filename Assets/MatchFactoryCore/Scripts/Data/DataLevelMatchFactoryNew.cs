using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Data
{
    [Serializable]
    public class DataLevelMatchFactoryNew
    {
        public int Level;
        public float TimeOnLevel;
        public List<DataItemLevel> TargetItems;
        public List<DataItemLevel> OtherItems;
    }

    [Serializable]
    public class DataItemLevel
    {
        public ItemFactoryType ItemType;
        public int Count;
        public Sprite ItemSprite;
    }
}