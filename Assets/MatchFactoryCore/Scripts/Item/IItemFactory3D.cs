using System;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public interface IItemFactory3D
    {
        public Rigidbody ObjectRigidbody { get; set; }
        public Collider ObjectCollider { get; set; }
        public Outline ObjectOutline { get; set; }
        public GameObject Prefab { get; set; }
        public float PrefabSize { get; set; }
        public float PrefabScale { get; set; }

        public void JumpFromBoard(Vector3 toTarget, Action onComplete = null);
        public void OnExplode();
        public void ChangeTo2D(Action onComplete = null);
    }
}