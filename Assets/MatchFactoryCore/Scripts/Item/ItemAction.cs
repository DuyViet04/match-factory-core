using System;
using MatchFactoryCore.Scripts.Data;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public class ItemAction : ItemEntity, IItem3D
    {
        #region Data

        public Rigidbody ObjectRigidbody { get; set; }
        public Collider ObjectCollider { get; set; }
        public ItemOutline ObjectOutline { get; set; }
        public GameObject Prefab { get; set; }
        public float PrefabSize { get; set; }
        public ActionType ActionType { get; set; }

        #endregion

        public void InitializeItemAction(InitItem3DContext context)
        {
            Id = context.Id;
            Prefab = context.Prefab;
            PrefabSize = context.PrefabSize;
            ActionType = context.ActionType;

            ObjectRigidbody = Prefab.GetComponent<Rigidbody>();
            ObjectCollider = Prefab.GetComponent<Collider>();
            ObjectOutline = Prefab.GetComponent<ItemOutline>();

            if (ObjectOutline != null)
            {
                ObjectOutline.enabled = false;
            }

            if (ObjectRigidbody != null && ObjectCollider != null)
            {
                Bounds bounds = ObjectCollider.bounds;
                float volume = bounds.size.x * bounds.size.y * bounds.size.z;
                ObjectRigidbody.mass = volume * PrefabSize;
            }
        }

        public void ActionBehaviour(Vector3 toTarget, Action onComplete = null)
        {
        }

        public void OnExplode()
        {
        }
    }
}