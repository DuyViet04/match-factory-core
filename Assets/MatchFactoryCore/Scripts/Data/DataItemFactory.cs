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
        public float prefabSize;
        public float prefabScale;

        [Header("2D")] public GameObject sprite;
        public float spriteScaleOnBar;
        public float spriteScaleWhenChange;

        public DataItemFactory()
        {
        }

        public DataItemFactory(DataItemFactory source)
        {
            itemFactoryType = source.itemFactoryType;
            itemFactoryCollectionType = source.itemFactoryCollectionType;
            prefab = source.prefab;
            prefabSize = source.prefabSize;
            sprite = source.sprite;
            prefabScale = source.prefabScale;
            spriteScaleOnBar = source.spriteScaleOnBar;
            spriteScaleWhenChange = source.spriteScaleWhenChange;
        }
    }
}