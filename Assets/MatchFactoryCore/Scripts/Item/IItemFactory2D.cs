using System.Collections.Generic;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public interface IItemFactory2D
    {
        public GameObject Sprite { get; set; }
        public int LastIndex { get; set; }
        public int CurrentIndex { get; set; }

        public void MoveToBar(GameObject sprite, List<GameObject> slots, Camera mainCam, List<int> data2Ds, int type,
            List<GameObject> allBarSprites);

        public void JumpOnBar(GameObject sprite, List<GameObject> slots, Camera mainCam, int numJump);
        public void Match(List<GameObject> matchs);
        public void ChangeTo3D();
    }
}