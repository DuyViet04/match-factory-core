using System.Collections.Generic;
using MatchFactoryCore.Scripts.Item;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Data
{
    [CreateAssetMenu(fileName = "InfoBoostersMatch3Factory", menuName = "GameMatchFactory/InfoBoostersMatch3Factory",
        order = 0)]
    public class InfoBoostersMatch3Factory : ScriptableObject, ISerializationCallbackReceiver
    {
        public SerializableDictionary<BoosterType, DataItemBooster> DictInfoBoostersMatch3Factory =
            new SerializableDictionary<BoosterType, DataItemBooster>();

        public Dictionary<BoosterType, DataItemBooster> CacheDictInfoBoostersMatch3Factory;

        public void SetCache()
        {
            CacheDictInfoBoostersMatch3Factory = new Dictionary<BoosterType, DataItemBooster>();
            foreach (var itemTemp in DictInfoBoostersMatch3Factory)
            {
                CacheDictInfoBoostersMatch3Factory.Add(itemTemp.Key, new DataItemBooster(itemTemp.Value));
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