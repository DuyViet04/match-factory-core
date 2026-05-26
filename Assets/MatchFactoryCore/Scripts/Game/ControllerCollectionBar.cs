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

        private static readonly int MaxCollectionBarSlots = 7;
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
            var itemFactory2DUi = newItemFactory2D.AddComponent<Image>();
            itemFactory2DUi.sprite = itemFactory2D.Sprite;
            itemFactory2DUi.SetNativeSize();
            setRectTransform(spriteTransform);

            HandleItemFactory2D(itemFactory2D);
        }

        private void HandleItemFactory2D(IItemFactory2D itemFactory2D)
        {
            InsertDataToDictionary(itemFactory2D, out var index);
            itemFactory2D.MoveToBar(collectionBarSlots, index);
            // itemFactory2D.JumpOnBar(collectionBarSlots, 1);
        }

        private void InsertDataToDictionary(IItemFactory2D itemFactory2D, out int index)
        {
            index = -1;
            if (_dictItemFactory2D[0] == null)
            {
                _dictItemFactory2D[0] = itemFactory2D;
                index = 0;
                return;
            }

            bool isInserted = false;
            for (int i = MaxCollectionBarSlots - 1; i >= 0; i--)
            {
                if (_dictItemFactory2D[i] == null) continue;
                if (_dictItemFactory2D[i].ItemFactoryType == itemFactory2D.ItemFactoryType)
                {
                    isInserted = true;
                    for (int j = MaxCollectionBarSlots - 1; j > i + 1; j--)
                    {
                        _dictItemFactory2D[j] = _dictItemFactory2D[j - 1];
                    }

                    _dictItemFactory2D[i + 1] = itemFactory2D;
                    index = i + 1;
                    break;
                }
            }

            if (!isInserted)
            {
                if (_dictItemFactory2D[MaxCollectionBarSlots - 1] != null) return;
                for (int i = 0; i < MaxCollectionBarSlots; i++)
                {
                    if (_dictItemFactory2D[i] != null) continue;
                    else
                    {
                        _dictItemFactory2D[i] = itemFactory2D;
                        index = i;
                        break;
                    }
                }
            }
        }
    }
}