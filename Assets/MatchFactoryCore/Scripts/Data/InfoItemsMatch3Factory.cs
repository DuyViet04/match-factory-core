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

        public Dictionary<ItemFactoryType, DataItemFactory> CacheDictInfoItemsMatch3Factory;

        public void SetCache()
        {
            CacheDictInfoItemsMatch3Factory = new Dictionary<ItemFactoryType, DataItemFactory>();
            foreach (var itemTemp in DictInfoItemsMatch3Factory)
            {
                CacheDictInfoItemsMatch3Factory.TryAdd(itemTemp.Key, new DataItemFactory(itemTemp.Value));
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