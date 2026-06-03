using System;
using System.Collections.Generic;
using System.Linq;
using MatchFactoryCore.Scripts.Game;
using MatchFactoryCore.Scripts.Game.State;
using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.Scripts.Item
{
    public enum BoosterType
    {
        Vacuum,
        Spring,
        Fan,
        FreezeGun
    }

    public class ItemBooster : ItemEntity
    {
        public Sprite BoosterSprite { get; set; }
        public BoosterType BoosterType { get; set; }
        public int BoosterCount { get; private set; }
        public RectTransform RectTransform { get; set; }

        [SerializeField] private int numberItemGet = 3;
        [SerializeField] private int freezeTime = 10;

        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private Text textCount;

        public event Action<List<IItem3D>, List<ItemFactory2D>, Vector3> OnVacuumBoosterUsed;
        public event Action<int> OnSpringBoosterUsed;
        public event Action<int> OnFreezeGunBoosterUsed;

        public void InitializeBooster(Sprite boosterSprite, BoosterType boosterType, int boosterCount)
        {
            BoosterSprite = boosterSprite;
            BoosterType = boosterType;
            BoosterCount = boosterCount;

            RectTransform = transform as RectTransform;
            button.onClick.AddListener(HandleItemBoosterClick);
            image.sprite = boosterSprite;
            textCount.text = BoosterCount.ToString();
        }

        private void HandleItemBoosterClick()
        {
            MatchFactoryState currentState = ControllerMatchFactory.Ins.GetCurrentState();
            if (currentState != MatchFactoryState.Playing) return;
            switch (BoosterType)
            {
                case BoosterType.Vacuum:
                    Debug.Log("Vacuum");
                    HandleBoosterVacuum();
                    break;
                case BoosterType.Spring:
                    Debug.Log("Spring");
                    HandleBoosterSpring();
                    break;
                case BoosterType.Fan:
                    Debug.Log("Fan");
                    break;
                case BoosterType.FreezeGun:
                    Debug.Log("FreezeGun");
                    HandleBoosterFreezeGun();
                    break;
            }
        }

        private void HandleBoosterVacuum()
        {
            if (BoosterCount == 0) return;
            List<ItemFactory2D> target2DItems = ControllerMatchFactory.Ins.GetLastTargetItem2DOnBar();

            // TODO: Có thể chuyển về IItemFactory2D
            if (target2DItems.Count > 0)
            {
                int remainCount = numberItemGet - target2DItems.Count;
                List<IItem3D> randomTargetItems = ControllerMatchFactory.Ins.GetRandomItemByType(
                    target2DItems.FirstOrDefault()!.ItemFactory.ItemFactoryType, remainCount);
                if (randomTargetItems.Count >= remainCount)
                {
                    OnVacuumBoosterUsed?.Invoke(randomTargetItems, target2DItems, transform.position);
                    BoosterCount--;
                }
            }
            else
            {
                List<IItem3D> randomItems = ControllerMatchFactory.Ins.GetListItemRandomByBooster(numberItemGet);
                OnVacuumBoosterUsed?.Invoke(randomItems, null, transform.position);
                BoosterCount--;
            }

            textCount.text = BoosterCount.ToString();
        }

        private void HandleBoosterSpring()
        {
            if (BoosterCount == 0) return;
            ItemFactory2D lastItem2D = ControllerMatchFactory.Ins.GetLastItem2DOnBar();
            if (lastItem2D == null) return;

            OnSpringBoosterUsed?.Invoke(lastItem2D.Id);
            BoosterCount--;
            textCount.text = BoosterCount.ToString();
        }

        // TODO: 
        private void HandleBoosterFan()
        {
        }

        private void HandleBoosterFreezeGun()
        {
            if (BoosterCount == 0) return;

            OnFreezeGunBoosterUsed?.Invoke(freezeTime);
            BoosterCount--;
            textCount.text = BoosterCount.ToString();
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }
    }
}