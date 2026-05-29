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
            RectTransform.DOJump(position, 100, 1, 0.25f).OnComplete(() => { onComplete?.Invoke(); });
        }

        public void JumpMatch(Vector3 currentPos, Vector3 position, JumpTypeMatch jumpType)
        {
            _jumpOnMatchSequence = DOTween.Sequence();
            Vector3 high = new Vector3(0, 100, 0);
            switch (jumpType)
            {
                case JumpTypeMatch.Left:
                    _jumpOnMatchSequence.Append(RectTransform.DOMove(currentPos + high, 0.25f))
                        .Append(RectTransform.DOMove(position + high, 0.5f).SetEase(Ease.InBack));
                    break;
                case JumpTypeMatch.Center:
                    _jumpOnMatchSequence.Append(RectTransform.DOMove(currentPos + high, 0.25f))
                        .Append(RectTransform.DOMove(position + high, 0.5f).SetEase(Ease.InBack));
                    break;
                case JumpTypeMatch.Right:
                    _jumpOnMatchSequence.Append(RectTransform.DOMove(currentPos + high, 0.25f))
                        .Append(RectTransform.DOMove(position + high, 0.5f).SetEase(Ease.InBack));
                    break;
            }

            _jumpOnMatchSequence.OnComplete(() => { Destroy(gameObject); });
        }

        public void JumpAfterMatch(int fromIndex, int targetIndex, Func<int, Vector3> getSlotPosition,
            Action onComplete = null, Action<int> onJumpStep = null)
        {
            if (fromIndex == targetIndex)
            {
                onComplete?.Invoke();
                return;
            }

            // _jumpAfterMatchSequence?.Kill();
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
                        .OnComplete(() => { onJumpStep?.Invoke(capturedIndex); }));
            }

            _jumpAfterMatchSequence.OnComplete(() => { onComplete?.Invoke(); });
        }
    }
}