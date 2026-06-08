using System;
using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Item;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game
{
    public class ControllerItemBooster : MonoBehaviour
    {
        [SerializeField] private InfoBoostersMatch3Factory infoBoostersMatch3Factory;
        [SerializeField] private GameObject boosterButtonPrefab;
        [SerializeField] private GameObject itemBoosterHolder;

        readonly Dictionary<BoosterType, ItemBooster> _itemBoosterDictionary =
            new Dictionary<BoosterType, ItemBooster>();


        public event Action<List<IItem3D>, List<ItemFactory2D>, Vector3> OnVacuumBoosterUsed;
        public event Action<int> OnSpringBoosterUsed;
        public event Action OnFanBoosterUsed;
        public event Action<int> OnFreezeGunBoosterUsed;

        private void Awake()
        {
            SpawnBooster();
        }

        private void SpawnBooster()
        {
            foreach (var item in infoBoostersMatch3Factory.CacheDictInfoBoostersMatch3Factory)
            {
                DataItemBooster data = item.Value;
                GameObject newButton = Instantiate(boosterButtonPrefab, itemBoosterHolder.transform);
                ItemBooster itemBoosterComp = newButton.GetComponent<ItemBooster>();
                itemBoosterComp.InitializeBooster(data.boosterSprite, data.boosterType, data.boosterCount);
                _itemBoosterDictionary.Add(data.boosterType, itemBoosterComp);
                switch (data.boosterType)
                {
                    case BoosterType.Vacuum:
                        itemBoosterComp.OnVacuumBoosterUsed += OnVacuumBoosterUsed;
                        break;
                    case BoosterType.Spring:
                        itemBoosterComp.OnSpringBoosterUsed += OnSpringBoosterUsed;
                        break;
                    case BoosterType.Fan:
                        itemBoosterComp.OnFanBoosterUsed += OnFanBoosterUsed;
                        break;
                    case BoosterType.FreezeGun:
                        itemBoosterComp.OnFreezeGunBoosterUsed += OnFreezeGunBoosterUsed;
                        break;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var itemTemp in _itemBoosterDictionary)
            {
                switch (itemTemp.Key)
                {
                    case BoosterType.Vacuum:
                        itemTemp.Value.OnVacuumBoosterUsed -= OnVacuumBoosterUsed;
                        break;
                    case BoosterType.Spring:
                        itemTemp.Value.OnSpringBoosterUsed -= OnSpringBoosterUsed;
                        break;
                    case BoosterType.Fan:
                        itemTemp.Value.OnFanBoosterUsed -= OnFanBoosterUsed;
                        break;
                    case BoosterType.FreezeGun:
                        itemTemp.Value.OnFreezeGunBoosterUsed -= OnFreezeGunBoosterUsed;
                        break;
                }
            }
        }

        public void SpawnSpringBoosterVfx()
        {
            _itemBoosterDictionary[BoosterType.Spring].SpawnSpringBoosterVfx();
        }

        public void SpawnItem2DSpringBoosterVfx(Vector2 uiPosition)
        {
            _itemBoosterDictionary[BoosterType.Spring].SpawnItem2DSpringBoosterVfx(uiPosition);
        }

        public Vector2 GetSpringBoosterButtonPosition()
        {
            return _itemBoosterDictionary[BoosterType.Spring].RectTransform.position;
        }
    }
}