using System;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Data
{
    [Serializable]
    public class DataItemFactory
    {
        public ItemFactoryType itemFactoryType;
        public ItemFactoryCollectionType itemFactoryCollectionType;

        [Header("3D")] public GameObject prefab;
        public float size;

        [Header("2D")] public GameObject sprite;

        public DataItemFactory()
        {
        }

        public DataItemFactory(DataItemFactory source)
        {
            itemFactoryType = source.itemFactoryType;
            itemFactoryCollectionType = source.itemFactoryCollectionType;
            prefab = source.prefab;
            size = source.size;
            sprite = source.sprite;
        }
    }
}