using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.Scripts.Item
{
    public enum JumpTypeMatch
    {
        Left,
        Center,
        Right,
    }

    public class ItemFactory2D : MonoBehaviour
    {
        [SerializeField] private Vector3 jumpHigh = new Vector3(0f, 100f, 0f);
        [SerializeField] private float sortTime = 0.1f;
        public int Id { get; private set; }
        public IItemFactory2D ItemFactory { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public int IndexFromBar { get; set; }
        [SerializeField] private Image image;

        Sequence _jumpOnMatchSequence;
        Sequence _jumpAfterMatchSequence;
        Sequence _moveToVacuumSequence;
        Tween _jumpOnBarTween;
        Tween _jumpAfterMatchTween;

        public void Initialize(int id, IItemFactory2D itemFactory)
        {
            Id = id;
            ItemFactory = itemFactory;
            image.sprite = ItemFactory.Sprite;
            image.SetNativeSize();
            RectTransform = GetComponent<RectTransform>();
        }

        public bool IsAnimating =>
            (_jumpOnBarTween != null && _jumpOnBarTween.IsActive() && _jumpOnBarTween.IsPlaying()) ||
            (_jumpAfterMatchTween != null && _jumpAfterMatchTween.IsActive() && _jumpAfterMatchTween.IsPlaying()) ||
            (_jumpOnMatchSequence != null && _jumpOnMatchSequence.IsActive() && _jumpOnMatchSequence.IsPlaying()) ||
            (_jumpAfterMatchSequence != null && _jumpAfterMatchSequence.IsActive() && _jumpAfterMatchSequence.IsPlaying());
 
        public void SnapToPosition(Vector3 worldPosition)
        {
            _jumpOnBarTween?.Kill(false);
            _jumpOnBarTween = null;
            _jumpAfterMatchTween?.Kill(false);
            _jumpAfterMatchTween = null;
            _jumpOnMatchSequence?.Kill(false);
            _jumpOnMatchSequence = null;
            _jumpAfterMatchSequence?.Kill(false);
            _jumpAfterMatchSequence = null;
            RectTransform.position = worldPosition;
        }

        public void JumpOnBar(Vector3 position, Action onComplete = null)
        {
            _jumpOnBarTween?.Kill();
            _jumpOnBarTween = RectTransform.DOJump(position, 100, 1, 0.25f)
                .OnComplete(() =>
                {
                    _jumpOnBarTween = null; // clear trước để chained call không Kill nhầm
                    onComplete?.Invoke();
                })
                .OnKill(() => { RectTransform.position = position; });
        }

        public void JumpMatch(Vector3 currentPos, Vector3 matchPos, JumpTypeMatch jumpType, Action onComplete = null)
        {
            _jumpOnMatchSequence?.Kill();
            _jumpOnMatchSequence = DOTween.Sequence();

            switch (jumpType)
            {
                case JumpTypeMatch.Left:
                    _jumpOnMatchSequence
                        .Append(RectTransform.DOMove(currentPos + jumpHigh, 0.25f).SetEase(Ease.InBack))
                        .Append(RectTransform.DOMove(matchPos + jumpHigh, 0.25f).SetEase(Ease.InBack))
                        .OnComplete(() =>
                        {
                            onComplete?.Invoke();
                            Destroy(gameObject);
                        })
                        .OnKill(() =>
                        {
                            RectTransform.position = matchPos;
                            onComplete?.Invoke();
                        });
                    break;
                case JumpTypeMatch.Center:
                    _jumpOnMatchSequence
                        .SetDelay(0.1f).Append(RectTransform.DOMove(currentPos + jumpHigh, 0.25f).SetEase(Ease.InBack))
                        .Append(RectTransform.DOMove(matchPos, 0.25f).SetEase(Ease.InBack))
                        .OnComplete(() =>
                        {
                            onComplete?.Invoke();
                            Destroy(gameObject);
                        })
                        .OnKill(() =>
                        {
                            RectTransform.position = matchPos;
                            onComplete?.Invoke();
                        });
                    break;
                case JumpTypeMatch.Right:
                    _jumpOnMatchSequence
                        .Append(RectTransform.DOMove(currentPos + jumpHigh, 0.25f).SetEase(Ease.InBack))
                        .Append(RectTransform.DOMove(matchPos + jumpHigh, 0.25f).SetEase(Ease.InBack))
                        .OnComplete(() =>
                        {
                            onComplete?.Invoke();
                            Destroy(gameObject);
                        })
                        .OnKill(() =>
                        {
                            RectTransform.position = matchPos;
                            onComplete?.Invoke();
                        });
                    break;
            }
        }

        public void JumpAfterMatch(Vector3 targetPos, float delay, Action onComplete)
        {
            _jumpAfterMatchTween?.Kill();
            _jumpAfterMatchTween = RectTransform.transform.DOJump(targetPos, 100, 1, 0.1f)
                .SetDelay(delay)
                .OnComplete(() => onComplete?.Invoke());
        }

        //public void JumpAfterMatch(int fromIndex, int targetIndex, float delay, Func<int, Vector3> getSlotPosition,
        //    Action onComplete = null, Action<int> onJumpStep = null)
        //{
        //    if (fromIndex == targetIndex)
        //    {
        //        onComplete?.Invoke();
        //        return;
        //    }

        //    _jumpAfterMatchSequence?.Kill();
        //    _jumpAfterMatchSequence = DOTween.Sequence();

        //    float timePerStep = sortTime;
        //    int direction = targetIndex < fromIndex ? -1 : 1;

        //    int current = fromIndex;
        //    while (current != targetIndex)
        //    {
        //        current += direction;
        //        int capturedIndex = current;
        //        _jumpAfterMatchSequence.Append(
        //            RectTransform.DOJump(getSlotPosition(capturedIndex), 100, 1, timePerStep)
        //                .SetDelay(delay)
        //                .OnComplete(() => { onJumpStep?.Invoke(capturedIndex); })
        //        );
        //    }

        //    _jumpAfterMatchSequence.OnComplete(() => { onComplete?.Invoke(); })
        //        .OnKill(() =>
        //        {
        //            RectTransform.position = getSlotPosition(targetIndex);
        //            IndexFromBar = targetIndex;
        //            onComplete?.Invoke();
        //        });
        //}

        public void MoveToVacuum(Vector3 targetPos, float delay, Action onComplete = null)
        {
            _moveToVacuumSequence?.Kill();
            _moveToVacuumSequence = DOTween.Sequence();
            Vector3 startPos = RectTransform.position;
            Vector3 middlePos = (startPos + targetPos) * 0.5f;
            middlePos.x -= 100f;
            middlePos.y += 100f;
            Vector3[] path = { middlePos, targetPos };

            _moveToVacuumSequence.Append(RectTransform.DOPath(path, 0.75f, PathType.CatmullRom))
                .Join(RectTransform.DOScale(RectTransform.localScale * 0.5f, 0.75f))
                .SetDelay(delay)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    Destroy(gameObject);
                })
                .OnKill(() => { RectTransform.position = targetPos; });
        }
    }
}