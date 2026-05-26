using System;
using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public interface IItemFactory2D
    {
        public Sprite Sprite { get; set; }

        public ItemFactoryType ItemFactoryType { get; set; }
        public float SpriteScaleOnBar { get; set; }
        public float SpriteScaleWhenChange { get; set; }

        public void MoveToBar(List<RectTransform> collectionBarSlots, int targetIndex, Action onComplete = null);

        public void JumpOnBar(List<RectTransform> collectionBarSlots, int numJump, Action onComplete = null);

        public void Match(List<GameObject> itemSlots, Camera mainCam, List<IItemFactory2D> matchs,
            Action onMatched = null);

        public void ChangeTo3D();
    }
}