using System;
using MatchFactoryCore.Scripts.Data;
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
        public ActionType ActionType { get; set; }

        public void ActionBehaviour(Vector3 targetPos, Action onComplete = null);
        public void BlowByFanBooster(float maxX, float maxZ, Action onComplete = null);
        public void JumpToBooster(Vector3 targetPos, Action onComplete = null);
        public void JumpFromBooster(Vector3 targetPos, Action onComplete = null);

        public void Explode(Action onComplete = null);
    }
}