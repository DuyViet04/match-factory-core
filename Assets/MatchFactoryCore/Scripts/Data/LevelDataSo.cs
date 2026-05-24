using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Data
{
    [Serializable]
    public struct ObjectInLevel
    {
        public int number;
        public ItemType itemType;
    }
    
    [CreateAssetMenu(fileName = "LevelDataSo", menuName = "Game/LevelDataSo", order = 0)]
    public class LevelDataSo : ScriptableObject
    {
        public List<ObjectInLevel> levelTarget;
        public List<ObjectInLevel> otherObjects;
    }
}