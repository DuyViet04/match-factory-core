using System.Collections.Generic;
using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
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

        #endregion

        #region Data 2D Object

        public GameObject Sprite { get; set; }
        public int CurrentIndex { get; set; }

        #endregion

        Sequence _jumpSequence;

        public void Initialize(int id, ItemFactoryType factoryType, GameObject prefab, GameObject sprite, float size)
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
        }


        #region Behaviour 3D Object

        public void JumpFromBoard(Vector3 toTarget)
        {
            Debug.Log("JumpFromBoard");
            _jumpSequence = DOTween.Sequence();
            _jumpSequence.Join(Prefab.transform.DOJump(toTarget, 1, 1, 0.25f));
            _jumpSequence.Join(Prefab.transform.DORotate(Prefab.transform.rotation.eulerAngles, 0.25f));
        }

        public void OnExplode()
        {
        }

        public void ChangeTo2D()
        {
            Debug.Log("ChangeTo2D");
            Prefab.SetActive(false);
            Sprite.SetActive(true);
            Sprite.transform.position = Prefab.transform.position;
        }

        #endregion

        #region Behaviour 2D Object

        Sequence _matchSequence;

        public void MoveToBar(List<GameObject> slots, Camera mainCam, List<int> data2Ds)
        {
            int maxSlot = MatchFactoryController.MaxSlot;
            if (data2Ds.Count >= maxSlot) return;
            InsertData(data2Ds, (int)FactoryType, maxSlot, out var index);
            var pos = slots[index].transform.position;
            var worldPos = mainCam.ScreenToWorldPoint(pos) - new Vector3(0, 1, 0);
            Sprite.transform.DOMove(worldPos, 0.5f);
        }

        public void JumpOnBar(List<GameObject> slots, Camera mainCam, int numJump)
        {
            var pos = slots[CurrentIndex].transform.position;
            var worldPos = mainCam.ScreenToWorldPoint(pos) - new Vector3(0, 1, 0);
            Sprite.transform.DOJump(worldPos, 1, numJump, 0.5f);
        }

        public void Match(List<GameObject> itemSlots, Camera mainCam, List<IItemFactory2D> matchs)
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
                    }
                });
        }

        public void ChangeTo3D()
        {
        }

        #endregion

        void InsertData(List<int> data2Ds, int type, int maxSlot, out int index)
        {
            index = -1;

            if (data2Ds.Count == 0)
            {
                data2Ds.Add(type);
                index = 0;
                CurrentIndex = index;
                return;
            }

            bool isInsert = false;
            for (int i = data2Ds.Count - 1; i >= 0; i--)
            {
                if (data2Ds.Count >= maxSlot) break;
                if (data2Ds[i] == type)
                {
                    data2Ds.Insert(i + 1, type);
                    index = i + 1;
                    isInsert = true;
                    break;
                }
            }

            if (!isInsert)
            {
                data2Ds.Add(type);
                index = data2Ds.Count - 1;
            }

            CurrentIndex = index;
        }

        Vector3 GetWorldPosition(Vector3 screenPos, Camera mainCam)
        {
            return mainCam.ScreenToWorldPoint(screenPos);
        }
    }
}