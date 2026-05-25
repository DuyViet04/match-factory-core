using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game;
using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.UI
{
    public class LevelTargetUI : MonoBehaviour
    {
        [SerializeField] private MatchFactoryController controller;
        [SerializeField] private Text levelTargetText;
        
        private Dictionary<ItemFactoryType, int> _targetDict;

        private void OnEnable()
        {
            controller.OnClickTarget += UpdateTargetText;
        }

        private void OnDisable()
        {
            controller.OnClickTarget -= UpdateTargetText;
        }

        private void Awake()
        {
            _targetDict = controller.TargetDictionary;
            levelTargetText.text = "";
            foreach (var item in _targetDict)
            {
                levelTargetText.text += $"{item.Key}: {item.Value}\n";
            }
        }

        void UpdateTargetText(int currentTarget)
        {
            levelTargetText.text = "";
            foreach (var item in _targetDict)
            {
                levelTargetText.text += $"{item.Key}: {item.Value}\n";
            }
        }
    }
}