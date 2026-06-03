using MatchFactoryCore.Scripts.Game;
using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.UI
{
    public class TimeLevelUI : MonoBehaviour
    {
        [SerializeField] private Text timeLevelText;
        [SerializeField] private Text freezeTimeText;

        int _minValue;
        int _secValue;

        private void OnEnable()
        {
            ControllerMatchFactory.Ins.OnTimeLevelChanged += UpdateTimeLevel;
            ControllerMatchFactory.Ins.OnFreezeTimeChanged += UpdateFreezeTime;
        }

        private void OnDisable()
        {
            ControllerMatchFactory.Ins.OnTimeLevelChanged -= UpdateTimeLevel;
            ControllerMatchFactory.Ins.OnTimeLevelChanged -= UpdateFreezeTime;
        }

        private void Start()
        {
            float timeLevel = ControllerMatchFactory.Ins.TimeLevel;
            UpdateTimeLevel(timeLevel);
        }

        private void UpdateTimeLevel(float time)
        {
            _minValue = (int)time / 60;
            _secValue = (int)time % 60;
            timeLevelText.text = $"{_minValue} : {_secValue}";
        }

        private void UpdateFreezeTime(float freezeTime)
        {
            freezeTimeText.gameObject.SetActive(true);
            _minValue = (int)freezeTime / 60;
            _secValue = (int)freezeTime % 60;
            freezeTimeText.text = $"{_minValue} : {_secValue}";

            if (freezeTime <= 0)
            {
                freezeTimeText.gameObject.SetActive(false);
            }
        }
    }
}