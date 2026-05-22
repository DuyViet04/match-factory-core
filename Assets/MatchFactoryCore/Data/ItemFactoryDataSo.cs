using UnityEngine;

namespace MatchFactoryCore.Data
{
    [CreateAssetMenu(fileName = "ItemFactoryDataSo", menuName = "Game/ItemFactoryDataSo")]
    public class ItemFactoryDataSo : ScriptableObject
    {
        public ItemType itemType;
        public CollectionType collectionType;
        
        [Header("3D")] 
        public GameObject prefab;
        public float size;
        public float weight;

        [Header("2D")] 
        public GameObject sprite;
    }
}