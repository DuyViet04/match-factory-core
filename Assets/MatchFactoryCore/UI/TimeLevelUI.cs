using MatchFactoryCore.Scripts.Game;
using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.UI
{
    public class TimeLevelUI : MonoBehaviour
    {
        [SerializeField] private MatchFactoryController controller;
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

        void UpdateTimeLevel(float time)
        {
            _minValue = (int)time / 60;
            _secValue = (int)time % 60;
            timeLevelText.text = $"{_minValue} : {_secValue}";
        }
    }
}