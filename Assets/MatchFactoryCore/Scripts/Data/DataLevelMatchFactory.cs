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

        public SerializableDictionary<ActionType, int> DictItemAction = new SerializableDictionary<ActionType, int>();

        public Dictionary<ItemFactoryType, int> CacheDictLevelTarget;
        public Dictionary<ItemFactoryType, int> CacheDictOtherObjectInLevel;
        public Dictionary<ActionType, int> CacheDictItemAction;

        public DataLevelMatchFactory()
        {
        }

        public DataLevelMatchFactory(DataLevelMatchFactory source)
        {
            TimeLevel = source.TimeLevel;
            DictLevelTarget = source.DictLevelTarget;
            DictOtherObjectInLevel = source.DictOtherObjectInLevel;
            DictItemAction = source.DictItemAction;
            SetCache();
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

            CacheDictItemAction = new Dictionary<ActionType, int>();
            foreach (var itemTemp in DictItemAction)
            {
                CacheDictItemAction.TryAdd(itemTemp.Key, itemTemp.Value);
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