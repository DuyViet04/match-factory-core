using System;
using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public struct InitItemFactory3DContext
    {
        public int Id;

        public GameObject Prefab;
        public float PrefabSize;

        public Sprite Sprite;
        public ItemFactoryType ItemFactoryType;
        public float SpriteScaleOnBar;
    }

    public class ItemFactory : ItemEntity, IItemFactory3D, IItemFactory2D
    {
        // #region Entity
        //
        // public int Id { get; private set; }
        //
        // #endregion

        private float _scaleWhenJump = 1.25f;

        #region Data 3D Object

        Vector3 _basePrefabScale;
        Vector3 _prefabBaseRotation;
        float _weight;

        public Rigidbody ObjectRigidbody { get; set; }
        public Collider ObjectCollider { get; set; }
        public ItemOutline ObjectOutline { get; set; }
        public GameObject Prefab { get; set; }
        public float PrefabSize { get; set; }

        #endregion

        #region Data 2D Object

        public Sprite Sprite { get; set; }
        public ItemFactoryType ItemFactoryType { get; set; }
        public float SpriteScaleOnBar { get; set; }

        #endregion

        public void Initialize(InitItemFactory3DContext itemFactory3DContext)
        {
            Id = itemFactory3DContext.Id;

            Prefab = itemFactory3DContext.Prefab;
            PrefabSize = itemFactory3DContext.PrefabSize;

            Sprite = itemFactory3DContext.Sprite;
            ItemFactoryType = itemFactory3DContext.ItemFactoryType;
            SpriteScaleOnBar = itemFactory3DContext.SpriteScaleOnBar;

            ObjectRigidbody = Prefab.GetComponent<Rigidbody>();
            ObjectCollider = Prefab.GetComponent<Collider>();
            ObjectOutline = Prefab.GetComponent<ItemOutline>();
            if (ObjectOutline == null)
            {
                ObjectOutline = Prefab.AddComponent<ItemOutline>();
                ObjectOutline.enabled = false;
            }

            if (ObjectRigidbody != null && ObjectCollider != null)
            {
                Bounds bounds = ObjectCollider.bounds;
                float volume = bounds.size.x * bounds.size.y * bounds.size.z;
                ObjectRigidbody.mass = volume * PrefabSize;
                _weight = ObjectRigidbody.mass;
            }

            _basePrefabScale = Prefab.transform.localScale;
            _prefabBaseRotation = Prefab.transform.rotation.eulerAngles;
            // Debug.Log($"{Id} {FactoryType} {PrefabScale}");
        }

        #region Behaviour 3D Object

        Sequence _jumpSequence;

        public void JumpFromBoard(Vector3 toTarget, Action onComplete = null, Action<Vector3, float> changeTo2D = null)
        {
            Debug.Log("JumpFromBoard");
            Vector3 worldPos;
            float scale;
            _jumpSequence = DOTween.Sequence();
            _jumpSequence.Append(Prefab.transform.DOJump(toTarget, 1, 1, 0.5f));
            _jumpSequence.Join(Prefab.transform.DOScale(_basePrefabScale * _scaleWhenJump, 0.1f));
            _jumpSequence.Join(Prefab.transform.DORotate(_prefabBaseRotation, 0.1f));
            _jumpSequence.InsertCallback(0.1f, () =>
            {
                worldPos = Prefab.transform.position;
                scale = Prefab.transform.localScale.x;
                changeTo2D?.Invoke(worldPos, scale);
            });
            _jumpSequence.OnComplete(() => { onComplete?.Invoke(); });
        }

        public void OnExplode()
        {
        }

        public void ChangeTo2D(Action onComplete = null)
        {
            Debug.Log("ChangeTo2D");
            Prefab.SetActive(false);
            onComplete?.Invoke();
        }

        #endregion

        #region Behaviour 2D Object

        public void ChangeTo3D()
        {
        }

        #endregion
    }
}