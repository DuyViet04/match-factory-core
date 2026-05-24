using System.Collections.Generic;
using DG.Tweening;
using MatchFactoryCore.Scripts.Game;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public class ItemSprite : MonoBehaviour, IItemFactory2D
    {
        private Sequence _moveUpSequence;
        private Sequence _mergedSequence;

        #region 2D Object

        public GameObject Sprite { get; set; }
        public int LastIndex { get; set; }
        public int CurrentIndex { get; set; }

        public void MoveToBar(GameObject sprite, List<GameObject> slots, Camera mainCam, List<int> data2Ds, int type,
            List<GameObject> allBarSprites)
        {
            int maxSlot = MatchFactoryController.MaxSlot;
            if (data2Ds.Count >= maxSlot) return;
            InsertData(data2Ds, type, maxSlot, allBarSprites, out var index);
            var pos = slots[index].transform.position;
            var worldPos = mainCam.ScreenToWorldPoint(pos) - new Vector3(0, 1, 0);
            sprite.transform.DOMove(worldPos, 0.5f);
        }

        void InsertData(List<int> data2Ds, int type, int maxSlot, List<GameObject> allBarSprites, out int index)
        {
            index = -1;

            // Lưu LastIndex hiện tại cho toàn bộ sprite đang có trên bar
            foreach (var go in allBarSprites)
            {
                var s = go.GetComponent<ItemSprite>();
                s.LastIndex = s.CurrentIndex;
            }

            if (data2Ds.Count == 0)
            {
                data2Ds.Add(type);
                index = 0;
                CurrentIndex = 0;
                return;
            }

            bool isInsert = false;
            for (int i = data2Ds.Count - 1; i >= 0; i--)
            {
                if (data2Ds.Count >= maxSlot) break;
                if (data2Ds[i] == type)
                {
                    data2Ds.Insert(i + 1, type);
                    index = i + 1;
                    isInsert = true;
                    break;
                }
            }

            if (!isInsert)
            {
                data2Ds.Add(type);
                index = data2Ds.Count - 1;
            }

            CurrentIndex = index;

            foreach (var go in allBarSprites)
            {
                var s = go.GetComponent<ItemSprite>();
                if (s.LastIndex >= index)
                {
                    s.CurrentIndex = s.LastIndex + 1;
                }
            }
        }

        public void JumpOnBar(GameObject sprite, List<GameObject> slots, Camera mainCam, int numJump)
        {
            var pos = slots[CurrentIndex].transform.position;
            var worldPos = mainCam.ScreenToWorldPoint(pos) - new Vector3(0, 1, 0);
            sprite.transform.DOJump(worldPos, 1, numJump, 0.5f);
        }

        public void Match(List<GameObject> matchs)
        {
            _moveUpSequence = DOTween.Sequence();
            _mergedSequence = DOTween.Sequence();
            if (matchs.Count != 3) return;
            _moveUpSequence.Join(matchs[0].transform.DOMove(matchs[0].transform.position + Vector3.forward, 0.25f))
                .Join(matchs[1].transform.DOMove(matchs[1].transform.position + Vector3.forward, 0.25f))
                .Join(matchs[2].transform.DOMove(matchs[2].transform.position + Vector3.forward, 0.25f))
                .OnComplete(() =>
                {
                    _mergedSequence.Join(matchs[0].transform.DOMove(matchs[1].transform.position, 0.25f))
                        .Join(matchs[2].transform.DOMove(matchs[1].transform.position, 0.25f))
                        .OnComplete(() =>
                        {
                            foreach (var item in matchs)
                            {
                                item.SetActive(false);
                            }
                        });
                });
        }

        public void ChangeTo3D()
        {
        }

        #endregion
    }
}