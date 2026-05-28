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
        public int IndexFromBar { get; set; }
        [SerializeField] private Image image;
        private RectTransform _rectTransform;

        Sequence _jumpOnMatchSequence;

        public void Initialize(int id, IItemFactory2D itemFactory)
        {
            Id = id;
            ItemFactory = itemFactory;
            image.sprite = ItemFactory.Sprite;
            _rectTransform = GetComponent<RectTransform>();
        }

        public void JumpOnBar(Vector3 position)
        {
            _rectTransform.DOJump(position, 100, 1, 0.25f);
        }

        public void JumpMatch(Vector3 position, JumpTypeMatch jumpType)
        {
            _jumpOnMatchSequence = DOTween.Sequence();
            Vector3 high = new Vector3(0, 100, 0);
            switch (jumpType)
            {
                case JumpTypeMatch.Left:
                    _jumpOnMatchSequence.Append(_rectTransform.DOMove(_rectTransform.position + high, 0.25f)
                            .SetEase(Ease.InBack))
                        .Append(_rectTransform.DOMove(position, 0.25f).SetEase(Ease.InBack));
                    break;
                case JumpTypeMatch.Center:
                    _jumpOnMatchSequence.Append(_rectTransform.DOMove(_rectTransform.position + high, 0.25f)
                            .SetEase(Ease.InBack))
                        .Append(_rectTransform.DOMove(position, 0.25f).SetEase(Ease.InBack));
                    break;
                case JumpTypeMatch.Right:
                    _jumpOnMatchSequence.Append(_rectTransform.DOMove(_rectTransform.position + high, 0.25f)
                            .SetEase(Ease.InBack))
                        .Append(_rectTransform.DOMove(position, 0.25f).SetEase(Ease.InBack));
                    break;
            }

            _jumpOnMatchSequence.OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}