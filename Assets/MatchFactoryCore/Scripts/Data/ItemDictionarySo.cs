using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Data
{
    [Serializable]
    public struct Data
    {
        public ItemType itemType;
        public ItemFactoryDataSo itemData;
    }

    [CreateAssetMenu(fileName = "ItemDictionarySo", menuName = "Game/ItemDictionarySo", order = 0)]
    public class ItemDictionarySo : ScriptableObject
    {
        public List<Data> items;
    }
}