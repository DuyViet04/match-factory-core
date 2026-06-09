using System.Collections.Generic;
using System.Linq;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.UI;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game
{
    public class ControllerTargetCard : MonoBehaviour
    {
        [SerializeField] private InfoItemsMatch3Factory infoItemsMatch3Factory;
        [SerializeField] private InfoLevelsMatch3Factory infoLevelsMatch3Factory;
        [SerializeField] private TargetCardUi targetCardUiPrefab;

        private Dictionary<ItemFactoryType, TargetCardUi> _cardUiDict = new Dictionary<ItemFactoryType, TargetCardUi>();

        public void SpawnCardUi(int level)
        {
            var dataLevel = infoLevelsMatch3Factory.CacheDictInfoLevelsMatch3Factory[level];
            var dataItem = infoItemsMatch3Factory.CacheDictInfoItemsMatch3Factory;
            var dataTarget = dataLevel.CacheDictLevelTarget;

            foreach (var itemTemp in dataTarget)
            {
                TargetCardUi newTargetCardUi = Instantiate(targetCardUiPrefab, transform);
                Sprite targetSprite = dataItem[itemTemp.Key].sprite;
                int targetCount = itemTemp.Value;
                newTargetCardUi.Initialize(targetSprite, targetCount, itemTemp.Key);
                newTargetCardUi.OnTargetFinished += MoveTargetCard;
                _cardUiDict.TryAdd(itemTemp.Key, newTargetCardUi);
            }
        }

        private void MoveTargetCard(ItemFactoryType type)
        {
            for (int i = 0; i < _cardUiDict.Count; i++)
            {
                if (_cardUiDict.ElementAt(i).Key == type)
                {
                    for (int j = i + 1; j < _cardUiDict.Count; j++)
                    {
                        Vector3 targetPos = _cardUiDict.ElementAt(j - 1).Value.transform.position;
                        _cardUiDict.ElementAt(j).Value.Move(targetPos);
                    }
                }
            }

            _cardUiDict.Remove(type);
            Destroy(_cardUiDict[type].gameObject);
            _cardUiDict[type].OnTargetFinished -= MoveTargetCard;
        }
    }
}