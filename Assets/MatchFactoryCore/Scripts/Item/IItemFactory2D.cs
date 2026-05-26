using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public interface IItemFactory2D
    {
        public GameObject Sprite { get; set; }
        public float SpriteScaleOnBar { get; set; }
        public float SpriteScaleWhenChange { get; set; }

        public void MoveToBar(List<GameObject> slots, Camera mainCam, int targetIndex, Action onComplete = null);

        public void JumpOnBar(List<GameObject> slotsPosition, Camera mainCam, int numJump, Action onComplete = null);

        public void Match(List<GameObject> itemSlots, Camera mainCam, List<IItemFactory2D> matchs,
            Action onMatched = null);

        public void ChangeTo3D();
    }
}