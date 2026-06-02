using System;
using System.Collections.Generic;
using System.Linq;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game;
using MatchFactoryCore.Scripts.Game.State;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

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

        public Action<List<IItem3D>, List<ItemFactory2D>, Vector3> OnVacuumBoosterUse;
        public Action<int> OnSpringBoosterUse;
        public Action<int> OnFreezeGunBoosterUse;

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
                    Debug.Log("HutBui");
                    HandleBoosterHutBui();
                    break;
                case BoosterType.Spring:
                    Debug.Log("DemNhun");
                    break;
                case BoosterType.Fan:
                    Debug.Log("Quat");
                    break;
                case BoosterType.FreezeGun:
                    Debug.Log("Sung");
                    break;
            }
        }

        private void HandleBoosterHutBui()
        {
            if (BoosterCount == 0) return;
            BoosterCount--;
            List<ItemFactory2D> target2DItems = ControllerMatchFactory.Ins.GetLastTargetItem2DOnBar();

            // todo: co the chuyen ve iitem2d
            if (target2DItems.Count > 0)
            {
                int remainCount = numberItemGet - target2DItems.Count;
                List<IItem3D> randomTargetItems =
                    ControllerMatchFactory.Ins.GetRandomItemByType(
                        target2DItems.FirstOrDefault().ItemFactory.ItemFactoryType, remainCount);
                if (randomTargetItems.Count >= remainCount)
                {
                    for (int i = 0; i < target2DItems.Count; i++)
                    {
                        float delay = i * 0.1f;
                        target2DItems[i].MoveToHutBui(RectTransform.position, delay);
                    }

                    ControllerMatchFactory.Ins.PlaySequenceAfterUseHutBui();


                    for (int i = 0; i < randomTargetItems.Count; i++)
                    {
                        Vector3 targetPos = Camera.main.ScreenToWorldPoint(transform.position);
                        targetPos.y = 0;
                        int capturedIndex = i;
                        randomTargetItems[i].ActionBehaviour(targetPos,
                            () => { Destroy(randomTargetItems[capturedIndex].Prefab); });
                    }

                    OnVacuumBoosterUse?.Invoke(randomTargetItems, target2DItems, transform.position);
                    return;
                }
            }

            {
                List<IItem3D> randomItems = ControllerMatchFactory.Ins.GetListItemRandomByBooster(numberItemGet);

                for (int i = 0; i < randomItems.Count; i++)
                {
                    Vector3 targetPos = Camera.main.ScreenToWorldPoint(transform.position);
                    targetPos.y = 0;
                    randomItems[i].ActionBehaviour(targetPos);
                }
                
                OnVacuumBoosterUse?.Invoke(randomItems, null, transform.position);
            }

            textCount.text = BoosterCount.ToString();
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }
    }
}