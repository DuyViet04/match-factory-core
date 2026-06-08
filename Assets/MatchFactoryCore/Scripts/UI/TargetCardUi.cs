using System;
using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game;
using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.Scripts.UI
{
    public class TargetCardUi : MonoBehaviour
    {
        [SerializeField] private Camera mainCam;
        [SerializeField] private GameObject frontFace;
        [SerializeField] private GameObject backFace;
        [SerializeField] private Image targetImage;
        [SerializeField] private Text targetCount;

        [SerializeField] private ParticleSystem targetReduceVfx;
        [SerializeField] private ParticleSystem targetFinishVfx;

        [SerializeField] private Vector3 endRotation = new Vector3(0, 540, 0);
        [SerializeField] private float cardScale = 1.1f;

        public event Action<ItemFactoryType> OnTargetFinished;

        private ItemFactoryType _itemFactoryType;
        private bool _isFront = true;
        private bool _isFinishTarget = false;

        private void Awake()
        {
            mainCam = Camera.main;
        }

        private void OnEnable()
        {
            ControllerMatchFactory.Ins.OnLevelTargetChanged += UpdateTargetCount;
        }

        private void OnDisable()
        {
            ControllerMatchFactory.Ins.OnLevelTargetChanged -= UpdateTargetCount;
        }

        public void Initialize(Sprite targetSprite, int count, ItemFactoryType itemFactoryType)
        {
            targetImage.sprite = targetSprite;
            targetCount.text = count.ToString();
            _itemFactoryType = itemFactoryType;
        }

        private void Update()
        {
            if (_isFinishTarget)
            {
                FlipTargetCard();
            }
        }

        void UpdateTargetCount(ItemFactoryType itemFactoryType, int count)
        {
            if (_itemFactoryType == itemFactoryType)
            {
                targetReduceVfx.Play();
                targetCount.text = count.ToString();

                if (count == 0)
                {
                    targetFinishVfx.Play();
                    FinishTarget();
                }
            }
        }

        private void FlipTargetCard()
        {
            Vector3 referenceForward = mainCam.transform.forward;

            _isFront = Vector3.Dot(transform.forward, referenceForward) >= 0;

            if (_isFront)
            {
                frontFace.SetActive(true);
                backFace.SetActive(false);
            }
            else
            {
                backFace.SetActive(true);
                frontFace.SetActive(false);
            }
        }

        Sequence _targetFinishSequence;

        private void FinishTarget()
        {
            _isFinishTarget = true;
            _targetFinishSequence = DOTween.Sequence();
            RectTransform rectTransform = transform as RectTransform;
            _targetFinishSequence.Append(rectTransform.DOScale(rectTransform.transform.localScale * cardScale, 0.1f));
            _targetFinishSequence.Append(rectTransform.DOLocalRotate(endRotation, 2.5f, RotateMode.FastBeyond360));
            _targetFinishSequence.Append(rectTransform.DOScale(Vector3.zero, 0.15f));
            _targetFinishSequence.OnComplete(() =>
            {
                targetFinishVfx.Stop();
                _isFinishTarget = false;
                OnTargetFinished?.Invoke(_itemFactoryType);
            });
        }

        public void Move(Vector3 targetPosition)
        {
            transform.DOMove(targetPosition, 0.15f).SetDelay(0.1f);
        }
    }
}