using System;
using System.Collections.Generic;
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
        Vector3[] _cacheSlotPosition;

        private void Awake()
        {
            _cacheAnchorPosition = new Vector2[collectionBarSlots.Count];
            _cacheSlotPosition = new Vector3[collectionBarSlots.Count];
            for (int i = 0; i < collectionBarSlots.Count; i++)
            {
                _cacheAnchorPosition[i] = collectionBarSlots[i].anchoredPosition;
                _cacheSlotPosition[i] = collectionBarSlots[i].position;
            }
        }

        public void SpawnItemFactory2D(IItemFactory2D itemFactory2D, int id, Vector3 worldPos, float scale)
        {
            Sequence moveSequence = DOTween.Sequence();
            int indexToInsert = GetIndexToInsert(itemFactory2D.ItemFactoryType);
            Vector2 jumpPos = GetPositionJump2D(indexToInsert);
            Vector2 spawnPos = mainCamera.WorldToScreenPoint(worldPos);
            ItemFactory2D newItemFactory2D = Instantiate(itemFactoryUI, spawnPos, Quaternion.identity);

            newItemFactory2D.transform.localEulerAngles = Vector3.one * scale;
            newItemFactory2D.transform.SetParent(holder.transform);
            newItemFactory2D.Initialize(id, itemFactory2D);

            moveSequence.Append(newItemFactory2D.transform.DOMove(jumpPos, 0.4f))
                .Join(newItemFactory2D.transform.DOScale(itemFactory2D.SpriteScaleOnBar, 0.4f))
                .OnComplete((() =>
                {
                    newItemFactory2D.RectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    BounceBarSlot(indexToInsert);
                }));

            newItemFactory2D.gameObject.SetActive(true);
            HandleItemFactory2D(newItemFactory2D);
        }

        public Vector3 GetPositionTo3DJump(ItemFactoryType type)
        {
            return mainCamera.ScreenToWorldPoint(GetPositionJump2D(type));
        }

        private Vector2 GetPositionJump2D(ItemFactoryType type)
        {
            return _cacheSlotPosition[GetIndexToInsert(type)];
        }

        private Vector2 GetPositionJump2D(int index)
        {
            return _cacheSlotPosition[index];
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

        private void HandleItemFactory2D(ItemFactory2D itemChoose)
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

        private void SortAfterInsert(ItemFactory2D itemChoose)
        {
            int insertIndex = GetIndexToInsert(itemChoose.ItemFactory.ItemFactoryType);
            _itemFactory2DList.Insert(insertIndex, itemChoose);
            _itemFactory2DList[insertIndex].IndexFromBar = insertIndex;
            for (int i = insertIndex + 1; i < _itemFactory2DList.Count; i++)
            {
                int capturedIndex = i;
                _itemFactory2DList[i].IndexFromBar = i;
                _itemFactory2DList[i].JumpOnBar(collectionBarSlots[i].position,
                    () => { BounceBarSlot(capturedIndex); });
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
                        GetPositionJump2D(matchsList[1].IndexFromBar), JumpTypeMatch.Center);
                    matchsList[2].JumpMatch(GetPositionJump2D(matchsList[2].IndexFromBar),
                        GetPositionJump2D(matchsList[1].IndexFromBar), JumpTypeMatch.Right);
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
                ItemFactory2D itemFactory2D = _itemFactory2DList[i];
                int oldIndex = itemFactory2D.IndexFromBar;
                int newIndex = i;

                itemFactory2D.IndexFromBar = newIndex;

                itemFactory2D.JumpAfterMatch(oldIndex, newIndex, idx => GetPositionJump2D(idx), null,
                    BounceBarSlot);
            }
        }

        private void BounceBarSlot(int index)
        {
            collectionBarSlots[index].anchoredPosition = _cacheAnchorPosition[index];
            collectionBarSlots[index].DOPunchPosition(Vector2.down * 50, 0.1f, 5, 5).OnComplete(() =>
            {
                collectionBarSlots[index].anchoredPosition = _cacheAnchorPosition[index];
            });
        }

        [Obsolete]
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