using System;
using System.Collections.Generic;
using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public struct InitItemFactory3DContext
    {
        public int Id;
        public ItemFactoryType FactoryType;
        public GameObject Prefab;
        public Sprite Sprite;
        public float Size;
        public float PrefabScale;
        public Vector3 PrefabBaseRotation;
        public float SpriteScaleOnBar;
        public float SpriteScaleWhenChange;
    }

    public class ItemFactory : ItemEntity, IItemFactory3D, IItemFactory2D
    {
        #region Entity

        public int Id { get; private set; }

        #endregion

        #region Data 3D Object

        public Rigidbody RigidbodyObject { get; set; }
        public Collider ColliderObject { get; set; }
        public GameObject Prefab { get; set; }
        public float Size { get; set; }
        public float Weight { get; set; }
        public float PrefabScale { get; set; }
        public Vector3 PrefabBaseRotation { get; set; }

        #endregion

        #region Data 2D Object

        public Sprite Sprite { get; set; }
        public RectTransform SpriteTransform { get; set; }
        public ItemFactoryType ItemFactoryType { get; set; }
        public float SpriteScaleOnBar { get; set; }
        public float SpriteScaleWhenChange { get; set; }

        #endregion

        Sequence _jumpSequence;

        public void Initialize(InitItemFactory3DContext itemFactory3DContext)
        {
            Id = itemFactory3DContext.Id;
            ItemFactoryType = itemFactory3DContext.FactoryType;
            Prefab = itemFactory3DContext.Prefab;
            Sprite = itemFactory3DContext.Sprite;
            Size = itemFactory3DContext.Size;
            PrefabScale = itemFactory3DContext.PrefabScale;
            PrefabBaseRotation = itemFactory3DContext.PrefabBaseRotation;
            SpriteScaleOnBar = itemFactory3DContext.SpriteScaleOnBar;
            SpriteScaleWhenChange = itemFactory3DContext.SpriteScaleWhenChange;
            RigidbodyObject = Prefab.GetComponent<Rigidbody>();
            ColliderObject = Prefab.GetComponent<Collider>();
            if (RigidbodyObject != null && ColliderObject != null)
            {
                var b = ColliderObject.bounds;
                var volume = b.size.x * b.size.y * b.size.z;
                RigidbodyObject.mass = volume * Size;
                Weight = RigidbodyObject.mass;
            }

            _basePrefabScale = Prefab.transform.localScale;
            // Debug.Log($"{Id} {FactoryType} {PrefabScale}");
        }

        #region Behaviour 3D Object

        Vector3 _basePrefabScale;

        public void JumpFromBoard(Vector3 toTarget, Action onComplete)
        {
            Debug.Log("JumpFromBoard");
            _jumpSequence = DOTween.Sequence();
            _jumpSequence.Join(Prefab.transform.DOJump(toTarget, 1, 1, 0.25f));
            _jumpSequence.Join(Prefab.transform.DOScale(_basePrefabScale * PrefabScale, 0.25f));
            _jumpSequence.Join(Prefab.transform.DORotate(PrefabBaseRotation, 0.25f));
            _jumpSequence.OnComplete(() => onComplete?.Invoke());
        }

        public void OnExplode()
        {
        }

        public void ChangeTo2D(Action onComplete)
        {
            Debug.Log("ChangeTo2D");
            Prefab.SetActive(false);
            onComplete?.Invoke();
        }

        #endregion

        #region Behaviour 2D Object

        readonly Vector3 _baseSpriteScale = Vector3.one;
        Sequence _moveToBarSequence;
        Sequence _jumpOnBarSequence;
        Sequence _matchSequence;

        public void MoveToBar(List<RectTransform> collectionBarSlots, int targetIndex, Action onComplete = null)
        {
            _moveToBarSequence = DOTween.Sequence();
            var targetPos = collectionBarSlots[targetIndex].position;
            var slotTransform = collectionBarSlots[targetIndex];
            _moveToBarSequence.Join(SpriteTransform.DOMove(targetPos, 0.25f))
                .Join(SpriteTransform.DOScale(_baseSpriteScale * SpriteScaleOnBar, 0.25f))
                .Append(slotTransform.DOPunchPosition(Vector2.down * 50, 0.1f, 3, 0.5f))
                .OnComplete(() =>
                {
                    slotTransform.anchoredPosition = collectionBarSlots[targetIndex].anchoredPosition;
                    onComplete?.Invoke();
                });
        }

        public void JumpOnBar(List<RectTransform> collectionBarSlots, int targetIndex, Action onComplete = null)
        {
            _jumpOnBarSequence = DOTween.Sequence();
            var targetPos = collectionBarSlots[targetIndex].position;
            var slotTransform = collectionBarSlots[targetIndex];
            _jumpOnBarSequence
                .Join(SpriteTransform.DOJump(targetPos, 100, 1, 0.5f))
                .Append(slotTransform.DOPunchPosition(Vector2.down * 50, 0.1f, 3, 0.5f))
                .OnComplete(() =>
                {
                    slotTransform.anchoredPosition = collectionBarSlots[targetIndex].anchoredPosition;
                    onComplete?.Invoke();
                });
        }

        public void Match(List<RectTransform> collectionBarSlots, List<(int, IItemFactory2D)> dictMatchs,
            Action onMatched)
        {
            _matchSequence = DOTween.Sequence();

            if (dictMatchs.Count != 3) return;

            var collectionBarIndex1 = dictMatchs[0].Item1;
            var collectionBarIndex2 = dictMatchs[1].Item1;
            var collectionBarIndex3 = dictMatchs[2].Item1;
            var sprite1 = (ItemFactory)dictMatchs[0].Item2;
            var sprite2 = (ItemFactory)dictMatchs[1].Item2;
            var sprite3 = (ItemFactory)dictMatchs[2].Item2;

            var matchHigh = 100;
            var matchPos1 = collectionBarSlots[collectionBarIndex1].position + new Vector3(0, matchHigh, 0);
            var matchPos2 = collectionBarSlots[collectionBarIndex2].position + new Vector3(0, matchHigh, 0);
            var matchPos3 = collectionBarSlots[collectionBarIndex3].position + new Vector3(0, matchHigh, 0);

            _matchSequence.Append(sprite1.SpriteTransform.DOMove(matchPos1, 0.25f).SetEase(Ease.InBack))
                .Join(sprite2.SpriteTransform.DOMove(matchPos2, 0.25f).SetEase(Ease.InBack))
                .Join(sprite3.SpriteTransform.DOMove(matchPos3, 0.25f).SetEase(Ease.InBack))
                .Append(sprite1.SpriteTransform.DOMove(matchPos2, 0.5f).SetEase(Ease.InBack))
                .Join(sprite2.SpriteTransform.DOMove(matchPos2, 0.5f).SetEase(Ease.InBack))
                .Join(sprite3.SpriteTransform.DOMove(matchPos2, 0.5f).SetEase(Ease.InBack))
                .OnComplete(() => onMatched?.Invoke());
        }

        public void JumpOnBarWhenMatched(List<RectTransform> collectionBarSlots, int targetFinalIndex,
            Action onComplete = null)
        {
            _jumpOnBarSequence?.Kill();
            _jumpOnBarSequence = DOTween.Sequence();

            var timePerStep = 0.5f / 3;
            var startIndex = targetFinalIndex + 3 - 1;
            for (int step = startIndex; step >= targetFinalIndex; step--)
            {
                var capturedIndex = step;
                var targetPos = collectionBarSlots[capturedIndex].position;
                var slotTransform = collectionBarSlots[capturedIndex];
                _jumpOnBarSequence
                    .Append(SpriteTransform.DOJump(targetPos, 100, 1, timePerStep))
                    .Append(slotTransform.DOPunchPosition(Vector2.down * 50, 0.1f, 3, 0.5f))
                    .OnComplete(() =>
                    {
                        slotTransform.anchoredPosition = collectionBarSlots[capturedIndex].anchoredPosition;
                        onComplete?.Invoke();
                    });
            }

            _jumpOnBarSequence.OnComplete(() => onComplete?.Invoke());
        }

        public void ChangeTo3D()
        {
        }

        #endregion
    }
}