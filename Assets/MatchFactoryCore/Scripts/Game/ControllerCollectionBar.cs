using System;
using System.Collections.Generic;
using System.Linq;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Item;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.Scripts.Game
{
    public class ControllerCollectionBar : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private ItemFactory2D itemFactoryUI;
        [SerializeField] private List<RectTransform> collectionBarSlots;
        [SerializeField] private GameObject holder;

        public event Action<IItemFactory2D> OnItemMatched;
        public event Action<int> OnInsertItemCompleted;

        private const int MaxCollectionBarSlots = 7;
        private readonly Dictionary<int, ItemFactory2D> _dictItemFactory2D = new Dictionary<int, ItemFactory2D>();

        public void SpawnItemFactory2D(IItemFactory2D itemFactory2D, int id)
        {
            ItemFactory2D newItemFactory2D = Instantiate(itemFactoryUI, GetPositionJump2D(itemFactory2D.ItemFactoryType), Quaternion.identity);
            newItemFactory2D.transform.SetParent(holder.transform);
            newItemFactory2D.Initialize(id, itemFactory2D);
            newItemFactory2D.gameObject.SetActive(false);
            _dictItemFactory2D.TryAdd(id, newItemFactory2D);
            SortDictAfterInsert(newItemFactory2D);
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
                if (_dictItemFactory2D[i].ItemFactory.ItemFactoryType == type && i < _dictItemFactory2D.Count - 1)
                {
                    hasItemSame = true;
                }

                if (hasItemSame && _dictItemFactory2D[i].ItemFactory.ItemFactoryType != type)
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
            afterItemChoose.Add(itemChoose.Id,itemChoose);
            bool hasItemSame = false;
            for (int i = 0; i < _dictItemFactory2D.Count; i++)
            {
                if (_dictItemFactory2D[i].ItemFactory.ItemFactoryType == itemChoose.ItemFactory.ItemFactoryType && i < _dictItemFactory2D.Count - 1)
                {
                    hasItemSame = true;
                }

                if (hasItemSame && _dictItemFactory2D[i].ItemFactory.ItemFactoryType != itemChoose.ItemFactory.ItemFactoryType)
                {
                    afterItemChoose.Add(i, _dictItemFactory2D[i]);
                    
                }
                else
                {
                    beforeItemChoose.Add(i, _dictItemFactory2D[i]);
                    _dictItemFactory2D[i].JumpOnBar(collectionBarSlots[i+1].position);
                }
            }
            
            _dictItemFactory2D.Clear();
            _dictItemFactory2D.AddRange(beforeItemChoose);
            _dictItemFactory2D.AddRange(afterItemChoose);
        }

        private void HandleItemFactory2D(IItemFactory2D itemFactory2D, int id)
        {
            InsertDataToDictionary(itemFactory2D, out var index, out var shiftedIndices);
            
            foreach (var shiftedIndex in shiftedIndices)
            {
                _dictItemFactory2D[shiftedIndex + 1].ItemFactory.JumpOnBar(collectionBarSlots, shiftedIndex + 1);
            }

            CheckMatch(itemFactory2D, out var matchesList, out var isMatch);

            if (isMatch)
            {
                //TODO sua lai ten la jumpToMatch va vong for chay moi  thang se co kieu jump khac nhau
                itemFactory2D.ItemFactory.Match(collectionBarSlots, matchesList, () =>
                {
                    foreach (var item in matchesList)
                    {
                        var factory = (ItemFactory)item.Item2.ItemFactory;
                        Destroy(factory.SpriteTransform.gameObject);
                    }

                    SortDictAfterMatch(out var itemFactory2DMoves);

                    foreach (var item in itemFactory2DMoves)
                    {
                        item.Item1.JumpOnBarWhenMatched(collectionBarSlots, item.Item2);
                    }

                    //TODO xu ly xong moi ban
                    foreach (var item in matchesList)
                    {
                        _dictItemFactory2D[item.Item1] = null;
                        OnItemMatched?.Invoke(item.Item2.ItemFactory);
                    }
                });
            }
            else
            {
                if (_dictItemFactory2D[MaxCollectionBarSlots - 1] != null)
                    OnInsertItemCompleted?.Invoke(MaxCollectionBarSlots);
            }
        }

        private void InsertDataToDictionary(IItemFactory2D itemFactory2D, out int index, out List<int> shiftedIndices)
        {
            index = -1;
            shiftedIndices = new List<int>();

            if (_dictItemFactory2D[0] == null)
            {
                _dictItemFactory2D[0] = (ItemFactory2D)itemFactory2D;
                index = 0;
                return;
            }

            bool isInserted = false;
            for (int i = MaxCollectionBarSlots - 1; i >= 0; i--)
            {
                if (_dictItemFactory2D[MaxCollectionBarSlots - 1] != null) return;
                if (_dictItemFactory2D[i] == null) continue;
                if (_dictItemFactory2D[i].ItemFactory.ItemFactoryType == itemFactory2D.ItemFactory.ItemFactoryType)
                {
                    for (int j = MaxCollectionBarSlots - 1; j > i + 1; j--)
                    {
                        _dictItemFactory2D[j] = _dictItemFactory2D[j - 1];
                        if (_dictItemFactory2D[j - 1] != null)
                            shiftedIndices.Add(j - 1);
                    }

                    _dictItemFactory2D[i + 1] = itemFactory2D;
                    index = i + 1;
                    isInserted = true;
                    break;
                }
            }

            if (!isInserted)
            {
                if (_dictItemFactory2D[MaxCollectionBarSlots - 1] != null) return;
                for (int i = 0; i < MaxCollectionBarSlots; i++)
                {
                    if (_dictItemFactory2D[i] != null) continue;
                    _dictItemFactory2D[i] = itemFactory2D;
                    index = i;
                    break;
                }
            }
        }

        private void CheckMatch(ItemFactory2D itemFactory2D)
        {
            var dictMatchs = new List<ItemFactory2D>();
            var itemFactory2DType = itemFactory2D.ItemFactory.ItemFactoryType;
            for (int i = 0; i < MaxCollectionBarSlots; i++)
            {
                if (_dictItemFactory2D[i] == null) continue;
                if (_dictItemFactory2D[i] != null && _dictItemFactory2D[i].ItemFactory.ItemFactoryType == itemFactory2DType)
                {
                    dictMatchs.Add(_dictItemFactory2D[i]);
                }

                if (dictMatchs.Count == 3)
                {
                    dictMatchs[0].JumpMatch(dictMatchs[1].transform.position, JumpTypeMatch.Left);
                    dictMatchs[1].JumpMatch(dictMatchs[1].transform.position, JumpTypeMatch.Center);
                    dictMatchs[2].JumpMatch(dictMatchs[1].transform.position, JumpTypeMatch.Right);
                    break;
                }
                
            }
        
        }

        private void SortDictAfterMatch(out List<(IItemFactory2D, int)> itemFactory2DMoves)
        {
            itemFactory2DMoves = new List<(IItemFactory2D, int)>();
            for (int i = 0; i < MaxCollectionBarSlots - 3; i++)
            {
                if (_dictItemFactory2D[i] != null) continue;
                var closestItem = _dictItemFactory2D[i + 3];
                if (closestItem != null)
                {
                    _dictItemFactory2D[i] = closestItem;
                    // itemFactory2DMoves.Add((_dictItemFactory2D[i], i));
                    _dictItemFactory2D[i + 3] = null;
                }
            }
        }
        
        // private void JumpOnBar(List<RectTransform> collectionBarSlots, int targetIndex, Action onComplete = null)
        // {
        //     _jumpOnBarSequence = DOTween.Sequence();
        //     var targetPos = collectionBarSlots[targetIndex].position;
        //     var slotTransform = collectionBarSlots[targetIndex];
        //     _jumpOnBarSequence
        //         .Join(SpriteTransform.DOJump(targetPos, 100, 1, 0.25f))
        //         .Append(slotTransform.DOPunchPosition(Vector2.down * 50, 0.1f, 3, 0.25f))
        //         .OnComplete(() =>
        //         {
        //             slotTransform.anchoredPosition = collectionBarSlots[targetIndex].anchoredPosition;
        //             onComplete?.Invoke();
        //         });
        // }

        public void SetActiveItemChoose(int id)
        {
            if (_dictItemFactory2D.ContainsKey(id))
            {
                _dictItemFactory2D[id].gameObject.SetActive(true);
            }
        }
    }
}