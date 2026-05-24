using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public interface IItemFactory3D
    {
        public GameObject Prefab { get; set; }
        public float Size { get; set; }
        public float Weight { get; set; }

        public void JumpFromBoard(Vector3 toTarget);
        public void OnExplode();
        public void ChangeTo2D(out GameObject sprite);
    }
}