using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Item;
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
        private readonly List<ItemFactory2D> _itemFactory2DList = new List<ItemFactory2D>();
        // private readonly Dictionary<int, ItemFactory2D> _dictItemFactory2D = new Dictionary<int, ItemFactory2D>();

        public void SpawnItemFactory2D(IItemFactory2D itemFactory2D, int id)
        {
            Vector2 jumpPos = GetPositionJump2D(itemFactory2D.ItemFactoryType);

            ItemFactory2D newItemFactory2D = Instantiate(itemFactoryUI, jumpPos, Quaternion.identity);
            newItemFactory2D.transform.SetParent(holder.transform);
            newItemFactory2D.Initialize(id, itemFactory2D);
            newItemFactory2D.gameObject.SetActive(true);
            HandleItemFactory2D(newItemFactory2D);
        }

        public Vector3 GetPositionTo3DJump(ItemFactoryType type)
        {
            return mainCamera.ScreenToWorldPoint(GetPositionJump2D(type));
        }

        private Vector2 GetPositionJump2D(ItemFactoryType type)
        {
            return collectionBarSlots[GetIndexToInsert(type)].position;
        }

        private Vector2 GetPositionJump2D(int index)
        {
            return collectionBarSlots[index].position;
        }

        private int GetIndexToInsert(ItemFactoryType type)
        {
            bool canInsert = false;
            for (int i = 0; i < _itemFactory2DList.Count; i++)
            {
                if (_itemFactory2DList[i].ItemFactory.ItemFactoryType == type)
                {
                    canInsert = true;
                }

                if (canInsert && _itemFactory2DList[i].ItemFactory.ItemFactoryType != type)
                {
                    return i;
                }
            }

            return _itemFactory2DList.Count;
        }

        public void HandleItemFactory2D(ItemFactory2D itemChoose)
        {
            SortAfterInsert(itemChoose);

            bool isMatch = IsMatch(itemChoose);
            if (isMatch)
            {
                SortAfterMatch();
            }
            else
            {
                bool isFull = _itemFactory2DList.Count == MaxCollectionBarSlots;
                OnInsertItemCompleted?.Invoke(isFull);
            }
        }

        private void SortAfterInsert(ItemFactory2D itemChoose, Action onComplete = null)
        {
            int insertIndex = GetIndexToInsert(itemChoose.ItemFactory.ItemFactoryType);
            _itemFactory2DList.Insert(insertIndex, itemChoose);
            _itemFactory2DList[insertIndex].IndexFromBar = insertIndex;
            BounceBarSlot(insertIndex);
            for (int i = insertIndex + 1; i < _itemFactory2DList.Count; i++)
            {
                _itemFactory2DList[i].JumpOnBar(collectionBarSlots[i].position, () => { BounceBarSlot(i); });
                _itemFactory2DList[i].IndexFromBar = i;
            }
        }

        private bool IsMatch(ItemFactory2D itemChoose)
        {
            bool isMatch = false;
            List<int> idList = new List<int>();
            List<ItemFactory2D> matchsList = new List<ItemFactory2D>();
            for (int i = 0; i < _itemFactory2DList.Count; i++)
            {
                if (_itemFactory2DList[i].ItemFactory.ItemFactoryType == itemChoose.ItemFactory.ItemFactoryType)
                {
                    matchsList.Add(_itemFactory2DList[i]);
                }

                if (matchsList.Count == 3)
                {
                    for (int j = matchsList.Count - 1; j >= 0; j--)
                    {
                        idList.Add(matchsList[j].Id);
                        _itemFactory2DList.Remove(matchsList[j]);
                    }

                    matchsList[0].JumpMatch(GetPositionJump2D(matchsList[0].IndexFromBar),
                        GetPositionJump2D(matchsList[1].IndexFromBar), JumpTypeMatch.Left);
                    matchsList[1].JumpMatch(GetPositionJump2D(matchsList[1].IndexFromBar),
                        GetPositionJump2D(matchsList[1].IndexFromBar), JumpTypeMatch.Left);
                    matchsList[2].JumpMatch(GetPositionJump2D(matchsList[2].IndexFromBar),
                        GetPositionJump2D(matchsList[1].IndexFromBar), JumpTypeMatch.Left);
                    isMatch = true;

                    OnItemMatched?.Invoke(idList);
                    break;
                }
            }

            return isMatch;
        }

        private void SortAfterMatch()
        {
            for (int i = 0; i < _itemFactory2DList.Count; i++)
            {
                ItemFactory2D item = _itemFactory2DList[i];
                int oldIndex = item.IndexFromBar;
                int newIndex = i;

                item.IndexFromBar = newIndex;

                item.JumpAfterMatch(oldIndex, newIndex, idx => GetPositionJump2D(idx), null,
                    BounceBarSlot);
            }
        }

        public void BounceBarSlot(int index)
        {
            RectTransform cache = collectionBarSlots[index];
            collectionBarSlots[index].DOPunchPosition(Vector2.down * 50, 0.1f, 5, 5).OnComplete(() =>
            {
                collectionBarSlots[index].anchoredPosition = cache.anchoredPosition;
            });
        }

        // private Vector2 GetPositionJump2D(ItemFactoryType type)
        // {
        //     bool hasItemSame = false;
        //     for (int i = 0; i < _dictItemFactory2D.Count; i++)
        //     {
        //         var item = _dictItemFactory2D.ElementAt(i).Value;
        //         if (item.ItemFactory.ItemFactoryType == type)
        //         {
        //             hasItemSame = true;
        //         }
        //
        //         if (hasItemSame && item.ItemFactory.ItemFactoryType != type)
        //         {
        //             return collectionBarSlots[i].position;
        //         }
        //     }
        //
        //     return collectionBarSlots[_dictItemFactory2D.Count].position;
        // }

        // private void SortDictAfterInsert(ItemFactory2D itemChoose)
        // {
        //     _dictItemFactory2D.Remove(itemChoose.Id);
        //
        //     Dictionary<int, ItemFactory2D> beforeItemChoose = new Dictionary<int, ItemFactory2D>();
        //     Dictionary<int, ItemFactory2D> afterItemChoose = new Dictionary<int, ItemFactory2D>();
        //     afterItemChoose.Add(itemChoose.Id, itemChoose);
        //
        //     bool hasItemSame = false;
        //     for (int i = 0; i < _dictItemFactory2D.Count; i++)
        //     {
        //         var currentKey = _dictItemFactory2D.ElementAt(i).Key;
        //         var currentItem = _dictItemFactory2D.ElementAt(i).Value;
        //
        //         if (currentItem.ItemFactory.ItemFactoryType == itemChoose.ItemFactory.ItemFactoryType)
        //         {
        //             hasItemSame = true;
        //         }
        //
        //         if (hasItemSame && currentItem.ItemFactory.ItemFactoryType != itemChoose.ItemFactory.ItemFactoryType)
        //         {
        //             afterItemChoose.Add(currentKey, currentItem);
        //         }
        //         else
        //         {
        //             beforeItemChoose.Add(currentKey, currentItem);
        //         }
        //     }
        //
        //     _dictItemFactory2D.Clear();
        //     foreach (var kvp in beforeItemChoose)
        //     {
        //         _dictItemFactory2D.Add(kvp.Key, kvp.Value);
        //     }
        //
        //     foreach (var kvp in afterItemChoose)
        //     {
        //         _dictItemFactory2D.Add(kvp.Key, kvp.Value);
        //     }
        //
        //     for (int i = 0; i < _dictItemFactory2D.Count; i++)
        //     {
        //         var item = _dictItemFactory2D.ElementAt(i).Value;
        //         item.IndexFromBar = i;
        //         if (item != itemChoose)
        //         {
        //             item.JumpOnBar(collectionBarSlots[i].position);
        //         }
        //     }
        // }

        // private void CheckMatch(ItemFactory2D itemFactory2D)
        // {
        //     bool isFull = false;
        //     List<int> idList = new List<int>();
        //     List<ItemFactory2D> dictMatchs = new List<ItemFactory2D>();
        //     ItemFactoryType itemFactory2DType = itemFactory2D.ItemFactory.ItemFactoryType;
        //
        //     for (int i = 0; i < _dictItemFactory2D.Count; i++)
        //     {
        //         var item = _dictItemFactory2D.ElementAt(i).Value;
        //         if (item != null && item.ItemFactory.ItemFactoryType == itemFactory2DType)
        //         {
        //             dictMatchs.Add(item);
        //         }
        //     }
        //
        //     if (dictMatchs.Count == 3)
        //     {
        //         foreach (var item in dictMatchs)
        //         {
        //             idList.Add(item.Id);
        //             _dictItemFactory2D.Remove(item.Id);
        //         }
        //
        //         dictMatchs[0].JumpMatch(dictMatchs[1].transform.position, JumpTypeMatch.Left);
        //         dictMatchs[1].JumpMatch(dictMatchs[1].transform.position, JumpTypeMatch.Center);
        //         dictMatchs[2].JumpMatch(dictMatchs[1].transform.position, JumpTypeMatch.Right);
        //
        //         OnItemMatched?.Invoke(idList);
        //
        //         SortDictAfterMatch();
        //     }
        //
        //     if (_dictItemFactory2D.Count == MaxCollectionBarSlots)
        //     {
        //         isFull = true;
        //     }
        //
        //     OnInsertItemCompleted?.Invoke(isFull);
        // }
        // private void SortDictAfterMatch()
        // {
        //     ItemFactory2D[] itemFactory2DArray = new ItemFactory2D[MaxCollectionBarSlots];
        //
        //     for (int i = 0; i < _dictItemFactory2D.Count; i++)
        //     {
        //         itemFactory2DArray[i] = _dictItemFactory2D.ElementAt(i).Value;
        //     }
        //
        //     for (int i = 0; i < MaxCollectionBarSlots; i++)
        //     {
        //         if (i < MaxCollectionBarSlots - 1)
        //         {
        //             while (itemFactory2DArray[i] == null && itemFactory2DArray[i + 1] != null)
        //             {
        //                 itemFactory2DArray[i] = itemFactory2DArray[i + 1];
        //                 itemFactory2DArray[i + 1] = null;
        //                 itemFactory2DArray[i].IndexFromBar = i;
        //                 itemFactory2DArray[i].JumpOnBar(collectionBarSlots[i].position);
        //                 if (i >= 1) i--;
        //             }
        //         }
        //     }
        //
        //     _dictItemFactory2D.Clear();
        //     for (int i = 0; i < MaxCollectionBarSlots; i++)
        //     {
        //         if (itemFactory2DArray[i] != null)
        //         {
        //             _dictItemFactory2D.Add(itemFactory2DArray[i].Id, itemFactory2DArray[i]);
        //             itemFactory2DArray[i].IndexFromBar = i;
        //             itemFactory2DArray[i].JumpOnBar(collectionBarSlots[i].position);
        //         }
        //     }
        // }

        public void SetActiveItemChoose(int id, Action onComplete = null)
        {
            for (int i = 0; i < _itemFactory2DList.Count; i++)
            {
                if (_itemFactory2DList[i].Id == id)
                {
                    _itemFactory2DList[i].gameObject.SetActive(true);
                    onComplete?.Invoke();
                    break;
                }
            }
        }
    }
}