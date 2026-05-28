using System;
using System.Collections.Generic;
using MatchFactoryCore.Scripts.Item;
using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.Scripts.Game
{
    public class ControllerCollectionBar : MonoBehaviour
    {
        [SerializeField] private ControllerMatchFactory controllerMatchFactory;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private List<RectTransform> collectionBarSlots;
        [SerializeField] private GameObject holder;

        public event Action<IItemFactory2D> OnItemMatched;
        public event Action<int> OnInsertItemCompleted;

        private const int MaxCollectionBarSlots = 7;
        private readonly Dictionary<int, IItemFactory2D> _dictItemFactory2D = new Dictionary<int, IItemFactory2D>();

        private void Awake()
        {
            for (int i = 0; i < MaxCollectionBarSlots; i++)
            {
                _dictItemFactory2D.Add(i, null);
            }
        }

        public void SpawnItemFactory2D(IItemFactory2D itemFactory2D, Vector3 spawnPoint,
            Action<RectTransform> setRectTransform)
        {
            var screenPos = mainCamera.WorldToScreenPoint(spawnPoint);
            
            var newItemFactory2D = new GameObject("ItemFactory2D");
            var spriteTransform = newItemFactory2D.AddComponent<RectTransform>();
            newItemFactory2D.transform.SetParent(holder.transform);
            newItemFactory2D.transform.SetPositionAndRotation(screenPos, Quaternion.identity);
            newItemFactory2D.transform.localScale = Vector3.one * itemFactory2D.SpriteScaleWhenChange;
            
            var itemFactory2DUi = newItemFactory2D.AddComponent<Image>();
            itemFactory2DUi.sprite = itemFactory2D.Sprite;
            itemFactory2DUi.SetNativeSize();
            setRectTransform(spriteTransform);

            HandleItemFactory2D(itemFactory2D);
        }

        private void HandleItemFactory2D(IItemFactory2D itemFactory2D)
        {
            InsertDataToDictionary(itemFactory2D, out var index, out var shiftedIndices);
            CheckMatch(itemFactory2D, out var matchesList, out var isMatch);

            itemFactory2D.MoveToBar(collectionBarSlots, index, () => { });

            foreach (var shiftedIndex in shiftedIndices)
            {
                _dictItemFactory2D[shiftedIndex + 1].JumpOnBar(collectionBarSlots, shiftedIndex + 1);
            }

            if (isMatch)
            {
                foreach (var item in matchesList)
                {
                    _dictItemFactory2D[item.Item1] = null;
                    OnItemMatched?.Invoke(item.Item2);
                }

                SortDictAfterCheck(out var itemFactory2DMoves);

                itemFactory2D.Match(collectionBarSlots, matchesList, () =>
                {
                    foreach (var item in matchesList)
                    {
                        var factory = (ItemFactory)item.Item2;
                        Destroy(factory.SpriteTransform.gameObject);
                    }

                    foreach (var item in itemFactory2DMoves)
                    {
                        item.Item1.JumpOnBarWhenMatched(collectionBarSlots, item.Item2);
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
                _dictItemFactory2D[0] = itemFactory2D;
                index = 0;
                return;
            }

            bool isInserted = false;
            for (int i = MaxCollectionBarSlots - 1; i >= 0; i--)
            {
                if (_dictItemFactory2D[MaxCollectionBarSlots - 1] != null) return;
                if (_dictItemFactory2D[i] == null) continue;
                if (_dictItemFactory2D[i].ItemFactoryType == itemFactory2D.ItemFactoryType)
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

        private void CheckMatch(IItemFactory2D itemFactory2D, out List<(int, IItemFactory2D)> dictMatchs,
            out bool isMatch)
        {
            dictMatchs = new List<(int, IItemFactory2D)>();
            isMatch = false;
            var itemFactory2DType = itemFactory2D.ItemFactoryType;
            for (int i = 0; i < MaxCollectionBarSlots; i++)
            {
                if (_dictItemFactory2D[i] == null) continue;
                if (_dictItemFactory2D[i] != null && _dictItemFactory2D[i].ItemFactoryType == itemFactory2DType)
                {
                    dictMatchs.Add((i, _dictItemFactory2D[i]));
                }

                if (dictMatchs.Count == 3)
                {
                    isMatch = true;
                    break;
                }
            }
        }

        private void SortDictAfterCheck(out List<(IItemFactory2D, int)> itemFactory2DMoves)
        {
            itemFactory2DMoves = new List<(IItemFactory2D, int)>();
            for (int i = 0; i < MaxCollectionBarSlots - 3; i++)
            {
                if (_dictItemFactory2D[i] != null) continue;
                var closestItem = _dictItemFactory2D[i + 3];
                if (closestItem != null)
                {
                    _dictItemFactory2D[i] = closestItem;
                    itemFactory2DMoves.Add((_dictItemFactory2D[i], i));
                    _dictItemFactory2D[i + 3] = null;
                }
            }
        }
    }
}