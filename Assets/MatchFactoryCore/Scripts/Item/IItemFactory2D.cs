using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public interface IItemFactory2D
    {
        public GameObject Sprite { get; set; }

        public void MoveToBar(List<GameObject> slots, Camera mainCam, List<int> data2Ds);

        public void JumpOnBar(List<GameObject> slots, Camera mainCam, int numJump);
        public void Match(List<GameObject> itemSlots, Camera mainCam, List<IItemFactory2D> matchs);
        public void ChangeTo3D();
    }
}