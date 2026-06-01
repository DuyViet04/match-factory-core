using System;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public interface IItem3D
    {
        public Rigidbody ObjectRigidbody { get; set; }
        public Collider ObjectCollider { get; set; }
        public ItemOutline ObjectOutline { get; set; }
        public GameObject Prefab { get; set; }
        public float PrefabSize { get; set; }

        public void ActionBehaviour(Vector3 toTarget, Action onComplete = null);
        public void OnExplode();
    }
}