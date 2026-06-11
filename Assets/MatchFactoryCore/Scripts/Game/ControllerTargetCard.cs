using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.UI;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game
{
    public class ControllerTargetCard : MonoBehaviour
    {
        [SerializeField] private InfoItemsMatch3Factory infoItemsMatch3Factory;
        [SerializeField] private TargetCardUi targetCardUiPrefab;

        private Dictionary<ItemFactoryType, TargetCardUi> _cardUiDict = new Dictionary<ItemFactoryType, TargetCardUi>();

        public void SpawnCardUi(int level)
        {
            DataLevelMatchFactoryNew dataLevel = LevelDataLoader.LevelCache[level];
            var dataItem = infoItemsMatch3Factory.CacheDictInfoItemsMatch3Factory;

            if (dataLevel.TargetItems == null) return;

            foreach (DataItemLevel item in dataLevel.TargetItems)
            {
                if (item.Count <= 0) continue;
                if (!dataItem.TryGetValue(item.ItemType, out DataItemFactory dataItemFactory)) continue;

                TargetCardUi newTargetCardUi = Instantiate(targetCardUiPrefab, transform);
                newTargetCardUi.Initialize(dataItemFactory.sprite, item.Count, item.ItemType);
                newTargetCardUi.OnTargetFinished += MoveTargetCard;
                _cardUiDict.TryAdd(item.ItemType, newTargetCardUi);
            }
        }

        private void MoveTargetCard(ItemFactoryType type)
        {
            for (int i = 0; i < _cardUiDict.Count; i++)
            {
                var entry = System.Linq.Enumerable.ElementAt(_cardUiDict, i);
                if (entry.Key == type)
                {
                    for (int j = i + 1; j < _cardUiDict.Count; j++)
                    {
                        Vector3 targetPos = System.Linq.Enumerable.ElementAt(_cardUiDict, j - 1).Value.transform.position;
                        System.Linq.Enumerable.ElementAt(_cardUiDict, j).Value.Move(targetPos);
                    }
                    break;
                }
            }

            if (_cardUiDict.TryGetValue(type, out TargetCardUi card))
            {
                card.OnTargetFinished -= MoveTargetCard;
                Destroy(card.gameObject);
            }
            _cardUiDict.Remove(type);
        }
    }
}