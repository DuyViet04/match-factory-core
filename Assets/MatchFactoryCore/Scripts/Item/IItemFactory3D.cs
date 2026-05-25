using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public interface IItemFactory3D
    {
        public Rigidbody RigidbodyObject { get; set; }
        public Collider ColliderObject { get; set; }
        public GameObject Prefab { get; set; }
        public float Size { get; set; }
        public float Weight { get; set; }

        public void JumpFromBoard(Vector3 toTarget);
        public void OnExplode();
        public void ChangeTo2D();
    }
}