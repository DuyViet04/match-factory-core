using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Data
{
    public struct ObjectInLevel
    {
        public int Number;
        public ItemType ItemType;
    }
    
    [CreateAssetMenu(fileName = "LevelDataSo", menuName = "Game/LevelDataSo", order = 0)]
    public class LevelDataSo : ScriptableObject
    {
        public List<ObjectInLevel> LevelTarget;
        public List<ObjectInLevel> OtherObjects;
    }
}