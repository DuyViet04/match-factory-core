using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game;
using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.UI
{
    public class LevelTargetUI : MonoBehaviour
    {
        [SerializeField] private ControllerMatchFactory controller;
        [SerializeField] private Text levelTargetText;

        private void OnEnable()
        {
            controller.OnLevelTargetChanged += UpdateTargetText;
        }

        private void OnDisable()
        {
            controller.OnLevelTargetChanged -= UpdateTargetText;
        }

        void UpdateTargetText(Dictionary<ItemFactoryType, int> targetDict)
        {
            levelTargetText.text = "";
            foreach (var item in targetDict)
            {
                levelTargetText.text += $"{item.Key}: {item.Value}\n";
            }
        }
    }
}