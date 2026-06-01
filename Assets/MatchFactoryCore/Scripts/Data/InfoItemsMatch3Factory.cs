using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Data
{
    [CreateAssetMenu(fileName = "InfoItemsMatch3Factory", menuName = "GameMatchFactory/InfoItemsMatch3Factory",
        order = 0)]
    public class InfoItemsMatch3Factory : ScriptableObject, ISerializationCallbackReceiver
    {
        public SerializableDictionary<ItemFactoryType, DataItemFactory> DictInfoItemsMatch3Factory =
            new SerializableDictionary<ItemFactoryType, DataItemFactory>();

        public SerializableDictionary<ActionType, DataItemFactory> DictInfoItemActionsMatch3Factory =
            new SerializableDictionary<ActionType, DataItemFactory>();

        public Dictionary<ItemFactoryType, DataItemFactory> CacheDictInfoItemsMatch3Factory;
        public Dictionary<ActionType, DataItemFactory> CacheDictInfoItemActionsMatch3Factory;

        public void SetCache()
        {
            CacheDictInfoItemsMatch3Factory = new Dictionary<ItemFactoryType, DataItemFactory>();
            foreach (var itemTemp in DictInfoItemsMatch3Factory)
            {
                CacheDictInfoItemsMatch3Factory.TryAdd(itemTemp.Key, new DataItemFactory(itemTemp.Value));
            }

            CacheDictInfoItemActionsMatch3Factory = new Dictionary<ActionType, DataItemFactory>();
            foreach (var itemTemp in DictInfoItemActionsMatch3Factory)
            {
                CacheDictInfoItemActionsMatch3Factory.TryAdd(itemTemp.Key, new DataItemFactory(itemTemp.Value));
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            SetCache();
        }
    }
}