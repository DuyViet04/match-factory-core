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

        Dictionary<BoosterType, ItemBooster> _itemBoosterDictionary = new Dictionary<BoosterType, ItemBooster>();


        public Action<List<IItem3D>, List<ItemFactory2D>, Vector3> OnVacuumBoosterUse;
        public Action<int> OnSpringBoosterUse;
        public Action<int> OnFreezeGunBoosterUse;

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
                        OnVacuumBoosterUse = itemBoosterComp.OnVacuumBoosterUse;
                        break;
                    case BoosterType.Spring:

                        OnSpringBoosterUse = itemBoosterComp.OnSpringBoosterUse;
                        break;
                    case BoosterType.Fan:
                        Debug.Log("Quat");
                        break;
                    case BoosterType.FreezeGun:
                        Debug.Log("Sung");
                        break;
                }
            }
        }
    }
}