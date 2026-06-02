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
        public int Id { get; private set; }
        public IItemFactory2D ItemFactory { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public int IndexFromBar { get; set; }
        [SerializeField] private Image image;

        Sequence _jumpOnMatchSequence;
        Sequence _jumpAfterMatchSequence;
        Sequence _moveToHutBuiSequence;

        public void Initialize(int id, IItemFactory2D itemFactory)
        {
            Id = id;
            ItemFactory = itemFactory;
            image.sprite = ItemFactory.Sprite;
            image.SetNativeSize();
            RectTransform = GetComponent<RectTransform>();
        }

        public void JumpOnBar(Vector3 position, Action onComplete = null)
        {
            RectTransform.DOJump(position, 100, 1, 0.25f).OnComplete(() => { onComplete?.Invoke(); })
                .OnKill(() => { RectTransform.position = position; });
        }

        public void JumpMatch(Vector3 currentPos, Vector3 position, JumpTypeMatch jumpType, Action onComplete = null)
        {
            _jumpOnMatchSequence = DOTween.Sequence();

            Vector3 high = new Vector3(0, 100, 0);

            _jumpOnMatchSequence
                .Append(RectTransform.DOMove(currentPos + high, 0.25f))
                .Append(RectTransform.DOMove(position + high, 0.5f).SetEase(Ease.InBack))
                .OnComplete(() => { onComplete?.Invoke(); })
                .OnKill(() =>
                {
                    RectTransform.position = position;
                    onComplete?.Invoke();
                });
        }

        public void JumpAfterMatch(int fromIndex, int targetIndex, Func<int, Vector3> getSlotPosition,
            Action onComplete = null, Action<int> onJumpStep = null)
        {
            if (fromIndex == targetIndex)
            {
                onComplete?.Invoke();
                return;
            }

            _jumpAfterMatchSequence = DOTween.Sequence();

            int steps = Mathf.Abs(targetIndex - fromIndex);
            float timePerStep = 0.5f / steps;
            int direction = targetIndex < fromIndex ? -1 : 1;

            int current = fromIndex;
            while (current != targetIndex)
            {
                current += direction;
                int capturedIndex = current;
                _jumpAfterMatchSequence.Append(
                    RectTransform.DOJump(getSlotPosition(capturedIndex), 100, 1, timePerStep)
                        .OnComplete(() => { onJumpStep?.Invoke(capturedIndex); })
                        .OnKill(() =>
                        {
                            RectTransform.position = getSlotPosition(capturedIndex);
                            onComplete?.Invoke();
                        }));
            }

            _jumpAfterMatchSequence.OnComplete(() => { onComplete?.Invoke(); });
        }

        public void MoveToHutBui(Vector3 targetPos, float delay, Action onComplete = null)
        {
            _moveToHutBuiSequence = DOTween.Sequence();
            Vector3 startPos = RectTransform.position;
            Vector3 middlePos = (startPos + (targetPos - startPos) * 0.5f);
            middlePos.x -= 1f;
            middlePos.y += 1f;
            Vector3[] path = { middlePos, targetPos };

            _moveToHutBuiSequence.Append(RectTransform.DOPath(path, 0.75f).SetDelay(delay))
                .Insert(delay, RectTransform.DOScale(RectTransform.localScale * 0.5f, 0.75f - delay))
                .OnComplete(() =>
                {
                    Destroy(gameObject);
                    onComplete?.Invoke();
                });
        }
    }
}