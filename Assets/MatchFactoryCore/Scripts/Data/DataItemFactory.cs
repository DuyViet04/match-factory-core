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
        public ActionType actionType;

        [Header("2D")] public Sprite sprite;
        public float spriteScaleOnBar;

        public DataItemFactory()
        {
        }

        public DataItemFactory(DataItemFactory source)
        {
            itemFactoryType = source.itemFactoryType;
            itemFactoryCollectionType = source.itemFactoryCollectionType;
            prefab = source.prefab;
            prefabSize = source.prefabSize;
            actionType = source.actionType;
            sprite = source.sprite;
            spriteScaleOnBar = source.spriteScaleOnBar;
        }
    }
}