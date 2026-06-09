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

        public void JumpOnBar(Vector3 positon, Action onComplete = null, Action onKill = null)
        {
            bool isComplete = false;
            _jumpOnBarTween?.Kill();
            _jumpAfterMatchTween?.Kill();
            _jumpOnBarTween = RectTransform.DOJump(positon, 100, 1, 0.25f)
                .OnComplete(() =>
                {
                    isComplete = true;
                    onComplete?.Invoke();
                })
                .OnKill(() =>
                {
                    gameObject.SetActive(true);
                    RectTransform.position = positon;
                    onKill?.Invoke();
                    if (!isComplete) onComplete?.Invoke();
                });
        }

        public void JumpMatch(Vector3 currentPos, Vector3 matchPos, JumpTypeMatch jumpType, Action onComplete = null)
        {
            _jumpOnBarTween?.Kill();
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
                        })
                        .OnKill(() =>
                        {
                            Destroy(gameObject);
                        });
                    break;
                case JumpTypeMatch.Center:
                    _jumpOnMatchSequence
                        .SetDelay(0.1f)
                        .Append(RectTransform.DOMove(currentPos + jumpHigh, 0.25f).SetEase(Ease.InBack))
                        .Append(RectTransform.DOMove(matchPos, 0.25f).SetEase(Ease.InBack))
                        .OnComplete(() =>
                        {
                            onComplete?.Invoke();
                        })
                        .OnKill(() =>
                        {
                            Destroy(gameObject);
                        });
                    break;
                case JumpTypeMatch.Right:
                    _jumpOnMatchSequence
                        .Append(RectTransform.DOMove(currentPos + jumpHigh, 0.25f).SetEase(Ease.InBack))
                        .Append(RectTransform.DOMove(matchPos + jumpHigh, 0.25f).SetEase(Ease.InBack))
                        .OnComplete(() =>
                        {
                            onComplete?.Invoke();
                        })
                        .OnKill(() =>
                        {
                            Destroy(gameObject);
                        });
                    break;
            }
        }

        public void JumpAfterMatch(Vector3 targetPos, float delay, Action onComplete = null, Action onKill = null)
        {
            _jumpOnBarTween?.Kill();
            _jumpAfterMatchTween?.Kill();
            _jumpAfterMatchTween = RectTransform.transform.DOJump(targetPos, 100, 1, sortTime)
                .SetDelay(delay)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                })
                .OnKill(() =>
                {
                    gameObject.SetActive(true);
                    RectTransform.position = targetPos;
                    onKill?.Invoke();
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