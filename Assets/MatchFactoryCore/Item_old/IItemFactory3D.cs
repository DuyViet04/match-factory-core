using UnityEngine;

namespace MatchFactoryCore.Item_old
{
    public interface IItemFactory3D
    {
        public GameObject Prefab { get; }
        public float Size { get; }
        public float Weight { get; }
        public float ExplodeScaleBonus { get; }

        public void JumpFromBoard(Vector3 toTarget);
        public void OnExplode();
        public void ChangeTo2D();
    }
}