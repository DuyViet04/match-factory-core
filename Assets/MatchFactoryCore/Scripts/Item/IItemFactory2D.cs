using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public interface IItemFactory2D
    {
        public GameObject Sprite { get; set; }

        public void MoveToBar(GameObject sprite, List<GameObject> slots, Camera mainCam, List<int> data2Ds, int type,
            out List<GameObject> go2Ds);

        public void JumpOnBar(GameObject sprite, List<GameObject> slots, Camera mainCam, int numJump);
        public void Match();
        public void ChangeTo3D();
    }
}