using System.Collections.Generic;
using DG.Tweening;
using MatchFactoryCore.Scripts.Game;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public class ItemSprite : MonoBehaviour, IItemFactory2D
    {
        public GameObject Owner { get; set; }
        public int LastIndex { get; set; }
        public int CurrentIndex { get; set; }

        #region 2D Object

        public GameObject Sprite { get; set; }

        public void MoveToBar(GameObject sprite, List<GameObject> slots, Camera mainCam, List<int> data2Ds, int type,
            out List<GameObject> go2Ds)
        {
            go2Ds = new List<GameObject>();
            int maxSlot = MatchFactoryController.MaxSlot;
            if (data2Ds.Count >= maxSlot) return;
            InsertData(sprite, data2Ds, type, maxSlot, out var index, out var go2D);
            if (go2D.Count == 0) return;
            go2Ds = go2D;
            var pos = slots[index].transform.position;
            var worldPos = mainCam.ScreenToWorldPoint(pos) - new Vector3(0, 1, 0);
            sprite.transform.DOMove(worldPos, 0.5f);
        }

        void InsertData(GameObject sprite, List<int> data2Ds, int type, int maxSlot, out int index,
            out List<GameObject> go2Ds)
        {
            index = -1;
            go2Ds = new List<GameObject>();
            if (data2Ds.Count == 0)
            {
                data2Ds.Add(type);
                go2Ds.Add(sprite);
                index = 0;
                LastIndex = 0;
                return;
            }

            bool isInsert = false;
            for (int i = data2Ds.Count - 1; i >= 0; i--)
            {
                if (data2Ds.Count >= maxSlot) break;
                if (data2Ds[i] == type)
                {
                    data2Ds.Insert(i + 1, type);
                    go2Ds.Add(sprite);
                    isInsert = true;
                    index = i + 1;
                    LastIndex = i + 1;
                    break;
                }
            }

            if (!isInsert)
            {
                data2Ds.Add(type);
                go2Ds.Add(sprite);
                index = data2Ds.Count - 1;
                LastIndex = data2Ds.Count - 1;
            }

            for (int i = 0; i < go2Ds.Count; i++)
            {
                go2Ds[i].GetComponent<ItemSprite>().CurrentIndex = i;
            }
        }

        public void JumpOnBar(GameObject sprite, List<GameObject> slots, Camera mainCam, int numJump)
        {
            var pos = slots[CurrentIndex].transform.position;
            var worldPos = mainCam.ScreenToWorldPoint(pos) - new Vector3(0, 1, 0);
            sprite.transform.DOJump(worldPos, 1, numJump, 0.5f);
        }

        public void Match()
        {
            throw new System.NotImplementedException();
        }

        public void ChangeTo3D()
        {
        }

        #endregion
    }
}