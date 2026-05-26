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
        public int CurrentIndex { get; set; }

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
            _moveToBarSequence?.Kill();
            _moveToBarSequence = DOTween.Sequence();
            var targetPos = collectionBarSlots[targetIndex].position;
            _moveToBarSequence.Join(SpriteTransform.DOMove(targetPos, 0.25f))
                .Join(SpriteTransform.DOScale(_baseSpriteScale * SpriteScaleOnBar, 0.25f))
                .OnComplete(() => onComplete?.Invoke());
        }

        public void JumpOnBar(List<RectTransform> collectionBarSlots, int numJump, Action onComplete = null)
        {
            _jumpOnBarSequence?.Kill();
            _jumpOnBarSequence = DOTween.Sequence();
            var targetPos = collectionBarSlots[CurrentIndex].position;
            _jumpOnBarSequence
                .Join(SpriteTransform.DOJump(targetPos, 1, numJump, 0.5f))
                .OnComplete(() => onComplete?.Invoke());
        }

        public void Match(List<GameObject> itemSlots, Camera mainCam, List<IItemFactory2D> matchs, Action onMatched)
        {
            _matchSequence = DOTween.Sequence();

            if (matchs.Count != 3) return;
            var idx0 = ((ItemFactory)matchs[0]).CurrentIndex;
            var idx1 = ((ItemFactory)matchs[1]).CurrentIndex;
            var idx2 = ((ItemFactory)matchs[2]).CurrentIndex;
            var pos0 = GetWorldPosition(itemSlots[idx0].transform.position, mainCam) + Vector3.forward - Vector3.up;
            var pos1 = GetWorldPosition(itemSlots[idx1].transform.position, mainCam) + Vector3.forward - Vector3.up;
            var pos2 = GetWorldPosition(itemSlots[idx2].transform.position, mainCam) + Vector3.forward - Vector3.up;

            // _matchSequence.Append(matchs[0].Sprite.transform.DOMove(pos0, 0.25f))
            //     .Join(matchs[1].Sprite.transform.DOMove(pos1, 0.25f))
            //     .Join(matchs[2].Sprite.transform.DOMove(pos2, 0.25f))
            //     .Append(matchs[0].Sprite.transform.DOMove(pos1, 0.25f))
            //     .Join(matchs[1].Sprite.transform.DOMove(pos1, 0.25f))
            //     .Join(matchs[2].Sprite.transform.DOMove(pos1, 0.25f))
            //     .OnComplete(() =>
            //     {
            //         foreach (var item in matchs)
            //         {
            //             Destroy(((MonoBehaviour)item).gameObject);
            //             Debug.Log(((MonoBehaviour)item).gameObject.name);
            //             onMatched?.Invoke();
            //         }
            //     });
        }

        public void ChangeTo3D()
        {
        }

        #endregion


        Vector3 GetWorldPosition(Vector3 screenPos, Camera mainCam)
        {
            return mainCam.ScreenToWorldPoint(screenPos);
        }
    }
}