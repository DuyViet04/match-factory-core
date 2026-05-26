using System;
using System.Collections.Generic;
using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public struct InitContext
    {
        public int Id;
        public ItemFactoryType FactoryType;
        public GameObject Prefab;
        public GameObject Sprite;
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
        public ItemFactoryType FactoryType { get; private set; }

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

        public GameObject Sprite { get; set; }
        public float SpriteScaleOnBar { get; set; }
        public float SpriteScaleWhenChange { get; set; }
        public int CurrentIndex { get; set; }

        #endregion

        Sequence _jumpSequence;

        public void Initialize(InitContext context)
        {
            Id = context.Id;
            FactoryType = context.FactoryType;
            Prefab = context.Prefab;
            Sprite = context.Sprite;
            Size = context.Size;
            PrefabScale = context.PrefabScale;
            PrefabBaseRotation = context.PrefabBaseRotation;
            SpriteScaleOnBar = context.SpriteScaleOnBar;
            SpriteScaleWhenChange = context.SpriteScaleWhenChange;
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
            _baseSpriteScale = Sprite.transform.localScale;
            // Debug.Log($"{Id} {FactoryType} {PrefabScale}");
        }

        /* public void Initialize(int id, ItemFactoryType factoryType, GameObject prefab, GameObject sprite, float size)
        {
            Id = id;
            FactoryType = factoryType;
            Prefab = prefab;
            Sprite = sprite;
            Size = size;
            RigidbodyObject = Prefab.GetComponent<Rigidbody>();
            ColliderObject = Prefab.GetComponent<Collider>();
            if (RigidbodyObject != null && ColliderObject != null)
            {
                var b = ColliderObject.bounds;
                var volume = b.size.x * b.size.y * b.size.z;
                RigidbodyObject.mass = volume * size;
                Weight = RigidbodyObject.mass;
            }
            // Debug.Log($"{id}: {Prefab.name} {Sprite.name} {Size} {Weight}");
        } */


        #region Behaviour 3D Object

        Vector3 _basePrefabScale;

        public void JumpFromBoard(Vector3 toTarget, Action onComplete)
        {
            Debug.Log("JumpFromBoard");
            _jumpSequence = DOTween.Sequence();
            _jumpSequence.Join(Prefab.transform.DOJump(toTarget, 1, 1, 0.25f));
            _jumpSequence.Join(Prefab.transform.DOScale(_basePrefabScale * PrefabScale, 0.25f));
            _jumpSequence.Join(Prefab.transform.DORotate(PrefabBaseRotation, 0.25f));
            // Debug.Log(PrefabBaseRotation);
            _jumpSequence.OnComplete(() => onComplete?.Invoke());
        }

        public void OnExplode()
        {
        }

        public void ChangeTo2D(Action onComplete)
        {
            Debug.Log("ChangeTo2D");
            Prefab.SetActive(false);
            Sprite.SetActive(true);
            Sprite.transform.position = Prefab.transform.position;
            Sprite.transform.localScale = _baseSpriteScale * SpriteScaleWhenChange;
            // Debug.Log(_baseSpriteScale);
            onComplete?.Invoke();
        }

        #endregion

        #region Behaviour 2D Object

        Vector3 _baseSpriteScale;
        Sequence _moveToBarSequence;
        Sequence _jumpOnBarSequence;
        Sequence _matchSequence;

        public void MoveToBar(List<GameObject> slots, Camera mainCam, int targetIndex, Action onComplete = null)
        {
            _moveToBarSequence?.Kill();
            _moveToBarSequence = DOTween.Sequence();
            var pos = slots[targetIndex].transform.position;
            var worldPos = mainCam.ScreenToWorldPoint(pos) - new Vector3(0, 1, 0);
            _moveToBarSequence
                .Join(Sprite.transform.DOMove(worldPos, 0.25f))
                .Join(Sprite.transform.DOScale(_baseSpriteScale * SpriteScaleOnBar, 0.25f))
                .OnComplete(() => onComplete?.Invoke());
        }

        public void JumpOnBar(List<GameObject> slotsPosition, Camera mainCam, int numJump, Action onComplete = null)
        {
            _jumpOnBarSequence?.Kill();
            _jumpOnBarSequence = DOTween.Sequence();
            var pos = slotsPosition[CurrentIndex].transform.position;
            var worldPos = mainCam.ScreenToWorldPoint(pos) - new Vector3(0, 1, 0);
            _jumpOnBarSequence
                .Join(Sprite.transform.DOJump(worldPos, 1, numJump, 0.5f))
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

            _matchSequence.Append(matchs[0].Sprite.transform.DOMove(pos0, 0.25f))
                .Join(matchs[1].Sprite.transform.DOMove(pos1, 0.25f))
                .Join(matchs[2].Sprite.transform.DOMove(pos2, 0.25f))
                .Append(matchs[0].Sprite.transform.DOMove(pos1, 0.25f))
                .Join(matchs[1].Sprite.transform.DOMove(pos1, 0.25f))
                .Join(matchs[2].Sprite.transform.DOMove(pos1, 0.25f))
                .OnComplete(() =>
                {
                    foreach (var item in matchs)
                    {
                        Destroy(((MonoBehaviour)item).gameObject);
                        Debug.Log(((MonoBehaviour)item).gameObject.name);
                        onMatched?.Invoke();
                    }
                });
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