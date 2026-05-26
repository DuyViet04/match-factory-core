using System;
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
        public float PrefabScale { get; set; }

        public void JumpFromBoard(Vector3 toTarget, Action onComplete = null);
        public void OnExplode();
        public void ChangeTo2D(Action onComplete = null);
    }
}