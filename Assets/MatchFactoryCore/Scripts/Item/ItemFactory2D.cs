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
        [SerializeField] private float matchTime = 0.2f;
        [SerializeField] private float sortTime = 0.1f;
        public int Id { get; private set; }
        public IItemFactory2D ItemFactory { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public int IndexFromBar { get; set; }
        [SerializeField] private Image image;

        Sequence _jumpOnMatchSequence;
        Sequence _moveToVacuumSequence;
        Tween _jumpOnBarTween;
        Sequence _jumpAfterMatchSequence;

        public void Initialize(int id, IItemFactory2D itemFactory)
        {
            Id = id;
            ItemFactory = itemFactory;
            image.sprite = ItemFactory.Sprite;
            image.SetNativeSize();
            RectTransform = GetComponent<RectTransform>();
        }

        public void JumpOnBar(Vector3 position, Action onComplete = null, Action onKill = null)
        {
            bool isComplete = false;
            _jumpOnBarTween?.Kill();
            _jumpOnBarTween = RectTransform.DOJump(position, 100, 1, 0.25f)
                .OnComplete(() =>
                {
                    isComplete = true;
                    onComplete?.Invoke();
                })
                .OnKill(() =>
                {
                    gameObject.SetActive(true);
                    RectTransform.position = position;
                    onKill?.Invoke();
                    if (!isComplete) onComplete?.Invoke();
                });
        }

        public void SnapToPosition(Vector3 position)
        {
            _jumpAfterMatchSequence?.Kill(false);
            _jumpOnBarTween?.Kill(false);
            RectTransform.position = position;
        }

        public void JumpMatch(Vector3 currentPos, Vector3 matchPos, JumpTypeMatch jumpType, Action onComplete = null)
        {
            _jumpOnBarTween?.Kill();
            _jumpOnMatchSequence?.Kill();
            _jumpOnMatchSequence = DOTween.Sequence();

            switch (jumpType)
            {
                case JumpTypeMatch.Left:
                    _jumpOnMatchSequence
                        .Append(RectTransform.DOMove(currentPos + jumpHigh, matchTime).SetEase(Ease.InBack))
                        .Append(RectTransform.DOMove(matchPos + jumpHigh, matchTime).SetEase(Ease.InBack))
                        .OnComplete(() =>
                        {
                            onComplete?.Invoke();
                        })
                        .OnKill(() =>
                        {
                            RectTransform.position = matchPos;
                            Destroy(gameObject);

                        });
                    break;
                case JumpTypeMatch.Center:
                    _jumpOnMatchSequence
                        .SetDelay(0.1f)
                        .Append(RectTransform.DOMove(currentPos + jumpHigh, matchTime).SetEase(Ease.InBack))
                        .Append(RectTransform.DOMove(matchPos, matchTime).SetEase(Ease.InBack))
                        .OnComplete(() =>
                        {
                            onComplete?.Invoke();
                        })
                        .OnKill(() =>
                        {
                            RectTransform.position = matchPos;
                            Destroy(gameObject);

                        });
                    break;
                case JumpTypeMatch.Right:
                    _jumpOnMatchSequence
                        .Append(RectTransform.DOMove(currentPos + jumpHigh, matchTime).SetEase(Ease.InBack))
                        .Append(RectTransform.DOMove(matchPos + jumpHigh, matchTime).SetEase(Ease.InBack))
                        .OnComplete(() =>
                        {
                            onComplete?.Invoke();
                        })
                        .OnKill(() =>
                        {
                            RectTransform.position = matchPos;
                            Destroy(gameObject);

                        });
                    break;
            }
        }

        public void JumpAfterMatch(int fromIndex, int targetIndex, float delay, Func<int, Vector3> getSlotPosition,
            Action onComplete = null, Action<int> onJumpStep = null, Action onKill = null)
        {
            bool isComplete = false;
            if (fromIndex == targetIndex)
            {
                isComplete = true;
                onComplete?.Invoke();
                return;
            }

            _jumpAfterMatchSequence?.Kill();
            _jumpAfterMatchSequence = DOTween.Sequence();

            float timePerStep = sortTime;
            int direction = targetIndex < fromIndex ? -1 : 1;

            int current = fromIndex;
            while (current != targetIndex)
            {
                current += direction;
                int capturedIndex = current;
                _jumpAfterMatchSequence.Append(
                    RectTransform.DOJump(getSlotPosition(capturedIndex), 100, 1, timePerStep)
                        .SetDelay(delay)
                        .OnComplete(() => { onJumpStep?.Invoke(capturedIndex); })
                );
            }

            _jumpAfterMatchSequence.OnComplete(() =>
            {
                isComplete = true;
                onComplete?.Invoke();
            })
            .OnKill(() =>
            {
                onKill?.Invoke();
                if (isComplete) return;
                onComplete?.Invoke();
            });
        }

        public void MoveToVacuum(Vector3 targetPos, float delay, Action onComplete = null, Action onKill = null)
        {
            bool isComplete = false;
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
                    isComplete = true;
                    onComplete?.Invoke();
                })
                .OnKill(() =>
                {
                    RectTransform.position = targetPos;
                    onKill?.Invoke();
                    Destroy(gameObject);
                    if (!isComplete) onComplete?.Invoke();
                });
        }
    }
}