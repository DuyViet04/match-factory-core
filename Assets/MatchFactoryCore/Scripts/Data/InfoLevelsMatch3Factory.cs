using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Data
{
    [CreateAssetMenu(fileName = "InfoLevelsMatch3Factory", menuName = "GameMatchFactory/InfoLevelsMatch3Factory",
        order = 0)]
    public class InfoLevelsMatch3Factory : ScriptableObject, ISerializationCallbackReceiver
    {
        public SerializableDictionary<int, DataLevelMatchFactory> DictInfoLevelsMatch3Factory =
            new SerializableDictionary<int, DataLevelMatchFactory>();

        public Dictionary<int, DataLevelMatchFactory> CacheDictInfoLevelsMatch3Factory;

        public void SetCache()
        {
            CacheDictInfoLevelsMatch3Factory = new Dictionary<int, DataLevelMatchFactory>();
            foreach (var itemTemp in DictInfoLevelsMatch3Factory)
            {
                CacheDictInfoLevelsMatch3Factory.TryAdd(itemTemp.Key, new DataLevelMatchFactory(itemTemp.Value));
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