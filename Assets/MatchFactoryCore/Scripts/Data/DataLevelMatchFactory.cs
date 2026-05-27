using System;
using System.Collections.Generic;

namespace MatchFactoryCore.Scripts.Data
{
    [Serializable]
    public class DataLevelMatchFactory
    {
        public float TimeLevel;

        public SerializableDictionary<ItemFactoryType, int> DictLevelTarget =
            new SerializableDictionary<ItemFactoryType, int>();

        public SerializableDictionary<ItemFactoryType, int> DictOtherObjectInLevel =
            new SerializableDictionary<ItemFactoryType, int>();

        public Dictionary<ItemFactoryType, int> CacheDictLevelTarget;
        public Dictionary<ItemFactoryType, int> CacheDictOtherObjectInLevel;

        public DataLevelMatchFactory()
        {
        }

        public DataLevelMatchFactory(DataLevelMatchFactory source)
        {
            TimeLevel = source.TimeLevel;
            DictLevelTarget = source.DictLevelTarget;
            DictOtherObjectInLevel = source.DictOtherObjectInLevel;
        }

        void SetCache()
        {
            CacheDictLevelTarget = new Dictionary<ItemFactoryType, int>();
            foreach (var itemTemp in DictLevelTarget)
            {
                CacheDictLevelTarget.TryAdd(itemTemp.Key, itemTemp.Value);
            }

            CacheDictOtherObjectInLevel = new Dictionary<ItemFactoryType, int>();
            foreach (var itemTemp in DictOtherObjectInLevel)
            {
                CacheDictOtherObjectInLevel.TryAdd(itemTemp.Key, itemTemp.Value);
            }
        }
    }

    [Serializable]
    public struct ObjectInLevel
    {
        public int number;
        public ItemFactoryType itemFactoryType;
    }
}