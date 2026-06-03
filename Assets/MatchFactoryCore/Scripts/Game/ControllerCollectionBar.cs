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

        Vector2[] _cacheAnchorPosition;

        private void Awake()
        {
            _cacheAnchorPosition = new Vector2[collectionBarSlots.Count];
            for (int i = 0; i < collectionBarSlots.Count; i++)
            {
                _cacheAnchorPosition[i] = collectionBarSlots[i].anchoredPosition;
            }
        }

        public void SpawnItemFactory2D(IItemFactory2D itemFactory2D, int id)
        {
            int indexToInsert = GetIndexToInsert(itemFactory2D.ItemFactoryType);
            Vector2 spawnPos = GetPositionJump2D(indexToInsert);
            ItemFactory2D newItemFactory2D = Instantiate(itemFactoryUI, holder.transform);

            newItemFactory2D.Initialize(id, itemFactory2D);
            newItemFactory2D.RectTransform.pivot = new Vector2(0.5f, 0f);
            newItemFactory2D.RectTransform.position = spawnPos;
            newItemFactory2D.RectTransform.localScale = Vector3.one * newItemFactory2D.ItemFactory.SpriteScaleOnBar;
            newItemFactory2D.gameObject.SetActive(false);

            InsertItem(newItemFactory2D, out List<ItemFactory2D> itemMoves);
            CheckMatch(newItemFactory2D, out bool isMatch, out List<ItemFactory2D> matchsList);

            PlaySequenceInsertItem(itemMoves, () =>
            {
                if (isMatch)
                {
                    PlaySequenceMatchItem(matchsList, () =>
                    {
                        foreach (var item in matchsList)
                        {
                            if (item != null && item.gameObject != null)
                                Destroy(item.gameObject);
                        }

                        SortAfterMatch(out int[] oldIndices);
                        PlaySequenceSortAfterMatch(oldIndices, () =>
                        {
                            bool isFull = _itemFactory2DList.Count == MaxCollectionBarSlots;
                            OnInsertItemCompleted?.Invoke(isFull);
                        });
                    });
                }
                else
                {
                    bool isFull = _itemFactory2DList.Count == MaxCollectionBarSlots;
                    OnInsertItemCompleted?.Invoke(isFull);
                }
            });
        }

        private void InsertItem(ItemFactory2D itemChoose, out List<ItemFactory2D> itemMoves)
        {
            itemMoves = new List<ItemFactory2D>();
            int insertIndex = GetIndexToInsert(itemChoose.ItemFactory.ItemFactoryType);
            _itemFactory2DList.Insert(insertIndex, itemChoose);
            itemChoose.IndexFromBar = insertIndex;
            for (int i = insertIndex + 1; i < _itemFactory2DList.Count; i++)
            {
                _itemFactory2DList[i].IndexFromBar = i;
                itemMoves.Add(_itemFactory2DList[i]);
            }
        }

        private void PlaySequenceInsertItem(List<ItemFactory2D> itemMoves, Action onComplete = null)
        {
            if (itemMoves.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int completedCount = 0;
            foreach (var item2DTemp in itemMoves)
            {
                int targetIndex = item2DTemp.IndexFromBar;
                item2DTemp.JumpOnBar(GetPositionJump2D(targetIndex), () =>
                {
                    BounceBarSlot(targetIndex);
                    completedCount++;
                    if (completedCount == itemMoves.Count)
                        onComplete?.Invoke();
                });
            }
        }

        private void CheckMatch(ItemFactory2D itemChoose, out bool isMatch, out List<ItemFactory2D> matchsList)
        {
            isMatch = false;
            matchsList = new List<ItemFactory2D>();
            List<int> idList = new List<int>();

            for (int i = 0; i < _itemFactory2DList.Count; i++)
            {
                if (_itemFactory2DList[i].ItemFactory.ItemFactoryType == itemChoose.ItemFactory.ItemFactoryType)
                {
                    matchsList.Add(_itemFactory2DList[i]);
                }

                if (matchsList.Count == 3)
                {
                    foreach (var item2DTemp in matchsList)
                    {
                        idList.Add(item2DTemp.Id);
                        SetActiveItemChoose(item2DTemp.Id);
                        _itemFactory2DList.Remove(item2DTemp);
                    }

                    isMatch = true;
                    OnItemMatched?.Invoke(idList);
                    break;
                }
            }
        }

        // TODO: Xu ly 2 match cung luc, insert khi match
        private void PlaySequenceMatchItem(List<ItemFactory2D> matchsList, Action onComplete = null)
        {
            int completedCount = 0;
            int total = matchsList.Count;

            void OnOneComplete()
            {
                completedCount++;
                if (completedCount == total) onComplete?.Invoke();
            }

            matchsList[0].JumpMatch(matchsList[0].RectTransform.position,
                matchsList[1].RectTransform.position, JumpTypeMatch.Left, OnOneComplete);
            matchsList[1].JumpMatch(matchsList[1].RectTransform.position,
                matchsList[1].RectTransform.position, JumpTypeMatch.Center, OnOneComplete);
            matchsList[2].JumpMatch(matchsList[2].RectTransform.position,
                matchsList[1].RectTransform.position, JumpTypeMatch.Right, OnOneComplete);
        }

        private void SortAfterMatch(out int[] oldIndices)
        {
            oldIndices = new int[_itemFactory2DList.Count];
            for (int i = 0; i < _itemFactory2DList.Count; i++)
            {
                oldIndices[i] = _itemFactory2DList[i].IndexFromBar;
                _itemFactory2DList[i].IndexFromBar = i;
            }
        }

        private void PlaySequenceSortAfterMatch(int[] oldIndices, Action onComplete = null)
        {
            if (_itemFactory2DList.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int completedCount = 0;
            int total = _itemFactory2DList.Count;

            for (int i = 0; i < _itemFactory2DList.Count; i++)
            {
                int capturedOldIndex = oldIndices[i];
                int capturedNewIndex = i;
                _itemFactory2DList[i].JumpAfterMatch(capturedOldIndex, capturedNewIndex,
                    idx => GetPositionJump2D(idx),
                    () =>
                    {
                        completedCount++;
                        if (completedCount == total) onComplete?.Invoke();
                    },
                    BounceBarSlot);
            }
        }

        private void PlaySequenceAfterUseVacuum(Action onComplete = null)
        {
            if (_itemFactory2DList.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int completedCount = 0;
            int total = _itemFactory2DList.Count;

            for (int i = 0; i < _itemFactory2DList.Count; i++)
            {
                int capturedOldIndex = _itemFactory2DList[i].IndexFromBar;
                _itemFactory2DList[i].IndexFromBar = i;
                int capturedNewIndex = i;
                _itemFactory2DList[i].JumpAfterMatch(capturedOldIndex, capturedNewIndex,
                    idx => GetPositionJump2D(idx),
                    () =>
                    {
                        completedCount++;
                        if (completedCount == total) onComplete?.Invoke();
                    },
                    BounceBarSlot);
            }
        }

        public void MoveToVacuum(List<ItemFactory2D> list2D, Vector3 position)
        {
            List<int> idList = new List<int>();
            foreach (var item2DTemp in list2D)
            {
                if (_itemFactory2DList.Contains(item2DTemp))
                {
                    item2DTemp.MoveToVacuum(position);
                    idList.Add(item2DTemp.Id);
                    _itemFactory2DList.Remove(item2DTemp);
                }
            }

            OnItemMatched?.Invoke(idList);
            PlaySequenceAfterUseVacuum();
        }

        private void BounceBarSlot(int index)
        {
            collectionBarSlots[index].anchoredPosition = _cacheAnchorPosition[index];
            collectionBarSlots[index].DOPunchPosition(Vector2.down * 50, 0.1f, 5, 5).OnComplete(() =>
            {
                collectionBarSlots[index].anchoredPosition = _cacheAnchorPosition[index];
            });
        }

        public void HandleBoosterSpringUsed(int id)
        {
            ItemFactory2D itemFactory2D = GetItemFactory2DById(id);
            _itemFactory2DList.Remove(itemFactory2D);
            itemFactory2D.gameObject.SetActive(false);
        }

        #region Gets Sets

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

        public void SetActiveItemChoose(int id, Action onComplete = null)
        {
            foreach (var item2DTemp in _itemFactory2DList)
            {
                if (item2DTemp.Id == id)
                {
                    item2DTemp.gameObject.SetActive(true);
                    onComplete?.Invoke();
                    break;
                }
            }
        }

        public List<ItemFactory2D> GetLastTargetItem2DOnBar()
        {
            List<ItemFactory2D> result = new List<ItemFactory2D>();
            for (int i = _itemFactory2DList.Count - 1; i >= 0; i--)
            {
                bool isTarget = ControllerMatchFactory.Ins.CheckTargetItemById(_itemFactory2DList[i].Id);
                if (isTarget) result.Add(_itemFactory2DList[i]);
            }

            return result;
        }

        public ItemFactory2D GetLastItem2DOnBar()
        {
            return _itemFactory2DList.LastOrDefault();
        }

        public ItemFactory2D GetItemFactory2DById(int id)
        {
            foreach (var item2DTemp in _itemFactory2DList)
            {
                if (item2DTemp.Id == id)
                {
                    return item2DTemp;
                }
            }

            return null;
        }

        #endregion
    }
}