using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Data
{
    public struct Data
    {
        public ItemType ItemType;
        public GameObject Prefab;
        public GameObject Sprite;
    }
    
    [CreateAssetMenu(fileName = "ItemDictionarySo", menuName = "Game/ItemDictionarySo", order = 0)]
    public class ItemDictionarySo : ScriptableObject
    {
        public Dictionary<ItemType, GameObject> PrefabDictionary;
        public Dictionary<ItemType, GameObject> SpriteDictionary;
    }
}