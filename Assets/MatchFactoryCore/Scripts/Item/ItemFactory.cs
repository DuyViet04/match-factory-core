using System;
using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using UnityEngine;
using Random = UnityEngine.Random;

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

        [SerializeField] private float maxLength = 2;
        [SerializeField] private float maxHeight = 2;
        public Rigidbody ObjectRigidbody { get; set; }
        public Collider ObjectCollider { get; set; }
        public ItemOutline ObjectOutline { get; set; }
        public GameObject Prefab { get; set; }
        public float PrefabSize { get; set; }
        public ActionType ActionType { get; set; }

        private readonly float _explodeScale = 1.25f;

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
        Sequence _jumpToBoosterSequence;
        Sequence _jumpFromBoosterSequence;

        public void ActionBehaviour(Vector3 targetPos, Action onComplete = null)
        {
            Debug.Log("JumpFromBoard");
            _jumpSequence = DOTween.Sequence();
            _jumpSequence.Append(Prefab.transform.DOJump(targetPos, 5f, 1, 0.5f));
            // _jumpSequence.Join(Prefab.transform.DOScale(_basePrefabScale * _scaleWhenJump, 0.1f));
            _jumpSequence.Join(Prefab.transform.DORotate(_prefabBaseRotation, 0.5f));
            _jumpSequence.OnComplete(() =>
            {
                onComplete?.Invoke();
                Prefab.SetActive(false);
            });
        }

        public void BlowByFanBooster(float maxX, float maxZ, Action onComplete = null)
        {
            if (Prefab == null)
            {
                onComplete?.Invoke();
                return;
            }

            Vector3 startPos = Prefab.transform.position;

            Vector3 nextPos = startPos + Vector3.forward * Random.Range(0, maxLength) +
                              Vector3.up * Random.Range(0, maxHeight);
            nextPos.x = Mathf.Clamp(nextPos.x, -maxX, maxX);
            nextPos.z = Mathf.Clamp(nextPos.z, -maxZ, maxZ);

            Vector3 endPos = nextPos + Vector3.back * Random.Range(0, maxLength);
            endPos.x = Mathf.Clamp(endPos.x, -maxX, maxX);
            endPos.z = Mathf.Clamp(endPos.z, -maxZ, maxZ);

            Vector3[] path = { endPos, nextPos };

            Prefab.transform.DOPath(path, 0.75f, PathType.CatmullRom)
                .OnComplete(() => { onComplete?.Invoke(); });
        }

        public void JumpToBooster(Vector3 targetPos, float delay, Action onComplete = null)
        {
            Debug.Log("JumpToBooster");
            _jumpToBoosterSequence = DOTween.Sequence();
            _jumpToBoosterSequence.Append(Prefab.transform.DOJump(targetPos, 5f, 1, 0.75f))
                .Join(Prefab.transform.DORotate(_prefabBaseRotation, 0.75f))
                .SetDelay(delay)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    Destroy(gameObject);
                });
        }

        public void JumpFromBooster(Vector3 targetPos, Action onComplete = null)
        {
            Debug.Log("JumpFromBooster");
            _jumpFromBoosterSequence = DOTween.Sequence();
            _jumpFromBoosterSequence.Append(Prefab.transform.DOJump(targetPos, 2f, 1, 0.5f))
                .SetDelay(0.15f)
                .OnComplete(() => { onComplete?.Invoke(); });
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
    }
}