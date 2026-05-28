using System;
using System.Collections.Generic;
using System.Linq;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Item;
using Unity.VisualScripting;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game
{
    public class ControllerCollectionBar : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private ItemFactory2D itemFactoryUI;
        [SerializeField] private List<RectTransform> collectionBarSlots;
        [SerializeField] private GameObject holder;

        public event Action<List<int>> OnItemMatched;
        public event Action<bool> OnInsertItemCompleted;

        private const int MaxCollectionBarSlots = 7;
        private readonly Dictionary<int, ItemFactory2D> _dictItemFactory2D = new Dictionary<int, ItemFactory2D>();

        public void SpawnItemFactory2D(IItemFactory2D itemFactory2D, int id)
        {
            ItemFactory2D newItemFactory2D = Instantiate(itemFactoryUI,
                GetPositionJump2D(itemFactory2D.ItemFactoryType), Quaternion.identity);
            newItemFactory2D.transform.SetParent(holder.transform);
            newItemFactory2D.Initialize(id, itemFactory2D);
            newItemFactory2D.gameObject.SetActive(false);

            _dictItemFactory2D.TryAdd(id, newItemFactory2D);

            SortDictAfterInsert(newItemFactory2D);
            CheckMatch(newItemFactory2D);
            SortDictAfterMatch();
        }

        public Vector3 GetPositionTo3DJump(ItemFactoryType type)
        {
            return mainCamera.ScreenToWorldPoint(GetPositionJump2D(type));
        }

        private Vector2 GetPositionJump2D(ItemFactoryType type)
        {
            bool hasItemSame = false;
            for (int i = 0; i < _dictItemFactory2D.Count; i++)
            {
                if (_dictItemFactory2D.ElementAt(i).Value.ItemFactory.ItemFactoryType == type &&
                    i < _dictItemFactory2D.Count - 1)
                {
                    hasItemSame = true;
                }

                if (hasItemSame && _dictItemFactory2D.ElementAt(i).Value.ItemFactory.ItemFactoryType != type)
                {
                    return collectionBarSlots[i + 1].position;
                }
            }

            return collectionBarSlots[_dictItemFactory2D.Count].position;
        }

        private void SortDictAfterInsert(ItemFactory2D itemChoose)
        {
            Dictionary<int, ItemFactory2D> beforeItemChoose = new Dictionary<int, ItemFactory2D>();
            Dictionary<int, ItemFactory2D> afterItemChoose = new Dictionary<int, ItemFactory2D>();
            afterItemChoose.Add(itemChoose.Id, itemChoose);

            bool hasItemSame = false;
            for (int i = 0; i < _dictItemFactory2D.Count; i++)
            {
                if (_dictItemFactory2D.ElementAt(i).Value.ItemFactory.ItemFactoryType ==
                    itemChoose.ItemFactory.ItemFactoryType &&
                    i < _dictItemFactory2D.Count - 1)
                {
                    hasItemSame = true;
                }

                if (hasItemSame && _dictItemFactory2D.ElementAt(i).Value.ItemFactory.ItemFactoryType !=
                    itemChoose.ItemFactory.ItemFactoryType)
                {
                    afterItemChoose.Add(i, _dictItemFactory2D.ElementAt(i).Value);
                    _dictItemFactory2D.ElementAt(i).Value.JumpOnBar(collectionBarSlots[i + 1].position);
                    _dictItemFactory2D.ElementAt(i).Value.IndexFromBar = i;
                }
                else
                {
                    beforeItemChoose.Add(i, _dictItemFactory2D.ElementAt(i).Value);
                    _dictItemFactory2D.ElementAt(i).Value.IndexFromBar = i;
                }
            }

            _dictItemFactory2D.Clear();
            _dictItemFactory2D.AddRange(beforeItemChoose);
            _dictItemFactory2D.AddRange(afterItemChoose);
        }

        private void CheckMatch(ItemFactory2D itemFactory2D)
        {
            bool isFull = false;
            List<int> idList = new List<int>();
            List<ItemFactory2D> dictMatchs = new List<ItemFactory2D>();
            ItemFactoryType itemFactory2DType = itemFactory2D.ItemFactory.ItemFactoryType;
            for (int i = 0; i < _dictItemFactory2D.Count; i++)
            {
                if (_dictItemFactory2D.ElementAt(i).Value == null) continue;
                if (_dictItemFactory2D.ElementAt(i).Value != null &&
                    _dictItemFactory2D.ElementAt(i).Value.ItemFactory.ItemFactoryType == itemFactory2DType)
                {
                    dictMatchs.Add(_dictItemFactory2D[i]);
                    _dictItemFactory2D.Remove(_dictItemFactory2D.ElementAt(i).Key);
                }

                if (dictMatchs.Count == 3)
                {
                    foreach (var item in dictMatchs)
                    {
                        idList.Add(item.Id);
                        _dictItemFactory2D.Remove(item.Id);
                    }

                    dictMatchs[0].JumpMatch(dictMatchs[1].transform.position, JumpTypeMatch.Left);
                    dictMatchs[1].JumpMatch(dictMatchs[1].transform.position, JumpTypeMatch.Center);
                    dictMatchs[2].JumpMatch(dictMatchs[1].transform.position, JumpTypeMatch.Right);

                    OnItemMatched?.Invoke(idList);
                    break;
                }
            }

            if (_dictItemFactory2D.Count == MaxCollectionBarSlots)
            {
                isFull = true;
            }

            OnInsertItemCompleted?.Invoke(isFull);
        }

        private void SortDictAfterMatch()
        {
            ItemFactory2D[] itemFactory2DArray = new ItemFactory2D[MaxCollectionBarSlots];

            for (int i = 0; i < _dictItemFactory2D.Count; i++)
            {
                if (_dictItemFactory2D.ContainsKey(i))
                {
                    itemFactory2DArray[i] = _dictItemFactory2D[i];
                }
                else
                {
                    itemFactory2DArray[i] = null;
                }

                if (i < MaxCollectionBarSlots - 1)
                {
                    while (itemFactory2DArray[i] == null && itemFactory2DArray[i + 1] != null)
                    {
                        itemFactory2DArray[i] = itemFactory2DArray[i + 1];
                        itemFactory2DArray[i + 1] = null;
                        itemFactory2DArray[i].IndexFromBar = i;
                        itemFactory2DArray[i].JumpOnBar(collectionBarSlots[i + 1].position);
                        if (i >= 1) i--;
                    }
                }
            }
        }

        public void SetActiveItemChoose(int id)
        {
            if (_dictItemFactory2D.ContainsKey(id))
            {
                _dictItemFactory2D[id].gameObject.SetActive(true);
            }
        }
    }
}