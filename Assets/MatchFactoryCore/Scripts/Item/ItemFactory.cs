using System;
using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public struct InitItem3DContext
    {
        public int Id;

        public GameObject Prefab;
        public float PrefabSize;
        public ActionType ActionType;

        public Sprite Sprite;
        public ItemFactoryType ItemFactoryType;
        public float SpriteScaleOnBar;
    }

    public class ItemFactory : ItemEntity, IItem3D, IItemFactory2D
    {
        private float _scaleWhenJump = 1.25f;

        #region Data 3D Object

        public Rigidbody ObjectRigidbody { get; set; }
        public Collider ObjectCollider { get; set; }
        public ItemOutline ObjectOutline { get; set; }
        public GameObject Prefab { get; set; }
        public float PrefabSize { get; set; }
        public ActionType ActionType { get; set; }

        private float _explodeScale = 1.25f;

        Vector3 _basePrefabScale;
        Vector3 _prefabBaseRotation;

        #endregion

        #region Data 2D Object

        public Sprite Sprite { get; set; }
        public ItemFactoryType ItemFactoryType { get; set; }
        public float SpriteScaleOnBar { get; set; }

        #endregion

        public void Initialize(InitItem3DContext item3DContext)
        {
            Id = item3DContext.Id;

            Prefab = item3DContext.Prefab;
            PrefabSize = item3DContext.PrefabSize;
            ActionType = item3DContext.ActionType;

            Sprite = item3DContext.Sprite;
            ItemFactoryType = item3DContext.ItemFactoryType;
            SpriteScaleOnBar = item3DContext.SpriteScaleOnBar;

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

            _basePrefabScale = Prefab.transform.localScale;
            _prefabBaseRotation = Prefab.transform.rotation.eulerAngles;
            // Debug.Log($"{Id} {FactoryType} {PrefabScale}");
        }

        #region Behaviour 3D Object

        Sequence _jumpSequence;
        Sequence _explodeSequence;

        public void ActionBehaviour(Vector3 targetPos, Action onComplete = null)
        {
            Debug.Log("JumpFromBoard");
            _jumpSequence = DOTween.Sequence();
            _jumpSequence.Append(Prefab.transform.DOJump(targetPos, 5f, 1, 0.5f));
            // _jumpSequence.Join(Prefab.transform.DOScale(_basePrefabScale * _scaleWhenJump, 0.1f));
            _jumpSequence.Join(Prefab.transform.DORotate(_prefabBaseRotation, 0.5f));
            _jumpSequence.OnComplete(() =>
            {
                Prefab.SetActive(false);
                onComplete?.Invoke();
            });
        }

        public void JumpToBooster(Vector3 targetPos, Action onComplete = null)
        {
            throw new NotImplementedException();
        }

        public void Explode(Action onComplete = null)
        {
            Debug.Log("Explode");
            _explodeSequence = DOTween.Sequence();
            _explodeSequence.Append(Prefab.transform.DOScale(_basePrefabScale * _explodeScale, 0.25f))
                // .Join(Prefab.transform.DOShakePosition(0.25f))
                .Append(Prefab.transform.DOScale(Vector3.zero, 0.25f))
                .OnComplete(() =>
                {
                    Destroy(gameObject);
                    onComplete?.Invoke();
                });
        }

        #endregion

        #region Behaviour 2D Object

        public void ChangeTo3D()
        {
        }

        #endregion
    }
}