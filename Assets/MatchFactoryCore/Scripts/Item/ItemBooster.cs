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

        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private Text textCount;

        public event Action<List<IItem3D>, List<ItemFactory2D>, Vector3> OnVacuumBoosterUse;
        public event Action<int> OnSpringBoosterUse;
        public event Action<int> OnFreezeGunBoosterUse;

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
                    break;
                case BoosterType.Fan:
                    Debug.Log("Fan");
                    break;
                case BoosterType.FreezeGun:
                    Debug.Log("FreezeGun");
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
                    OnVacuumBoosterUse?.Invoke(randomTargetItems, target2DItems, transform.position);
                    BoosterCount--;
                }
            }
            else
            {
                List<IItem3D> randomItems = ControllerMatchFactory.Ins.GetListItemRandomByBooster(numberItemGet);
                OnVacuumBoosterUse?.Invoke(randomItems, null, transform.position);
                BoosterCount--;
            }

            textCount.text = BoosterCount.ToString();
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }
    }
}