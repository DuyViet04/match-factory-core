using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MatchFactoryCore.Scripts.UI
{
    public class TargetCardUi : MonoBehaviour
    {
        [SerializeField] private Camera mainCam;
        [SerializeField] private GameObject frontFace;
        [SerializeField] private GameObject backFace;
        [SerializeField] private Image targetImage;
        [SerializeField] private Text targetCount;

        [SerializeField] private Vector3 endRotation = new Vector3(0, 540, 0);
        [SerializeField] private float cardScale = 1.1f;

        private bool _isFront = true;
        private float count = 3;
        private bool _isAnimate = false;

        private void Update()
        {
            count -= Time.deltaTime;
            if (count <= 0 && !_isAnimate)
            {
                FinishTarget();
                count = 0;
                _isAnimate = true;
            }

            if (_isAnimate)
                FlipTargetCard();
        }

        private void FlipTargetCard()
        {
            Vector3 referenceForward = mainCam.transform.forward;

            _isFront = Vector3.Dot(transform.forward, referenceForward) <= 0;

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
            _targetFinishSequence = DOTween.Sequence();
            RectTransform rectTransform = transform as RectTransform;
            _targetFinishSequence.Append(rectTransform.DOScale(rectTransform.transform.localScale * cardScale, 0.1f));
            _targetFinishSequence.Append(rectTransform.DORotate(endRotation, 2.5f, RotateMode.FastBeyond360));
            _targetFinishSequence.Append(rectTransform.DOScale(Vector3.zero, 0.15f));
        }
    }
}
