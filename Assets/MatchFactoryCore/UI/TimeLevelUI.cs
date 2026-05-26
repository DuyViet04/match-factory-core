using MatchFactoryCore.Scripts.Game;
using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.UI
{
    public class TimeLevelUI : MonoBehaviour
    {
        [SerializeField] private ControllerMatchFactory controller;
        [SerializeField] private Text timeLevelText;

        // Cache
        private int _minValue;
        private int _secValue;

        private void OnEnable()
        {
            controller.OnTimeLevelChanged += UpdateTimeLevel;
        }

        private void OnDisable()
        {
            controller.OnTimeLevelChanged -= UpdateTimeLevel;
        }

        private void Start()
        {
            UpdateTimeLevel(controller.TimeLevel);
        }

        void UpdateTimeLevel(float time)
        {
            _minValue = (int)time / 60;
            _secValue = (int)time % 60;
            timeLevelText.text = $"{_minValue} : {_secValue}";
        }
    }
}