using System;
using System.Collections.Generic;
using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MatchFactoryCore.Scripts.Item
{
    public struct InitItemActionContext
    {
        public int Id;
        public ActionType ActionType;
        public GameObject Prefab;
        public float PrefabSize;
    }

    public class ItemAction : ItemEntity, IItem3D
    {
        #region Data

        [SerializeField] private float maxLength = 2;
        [SerializeField] private float maxHeight = 2;
        [SerializeField] private int maxItemCount = 3;
        [SerializeField] private int timeBonus = 10;
        public Rigidbody ObjectRigidbody { get; set; }
        public Collider ObjectCollider { get; set; }
        public ItemOutline ObjectOutline { get; set; }
        public GameObject Prefab { get; set; }
        public float PrefabSize { get; set; }
        public ActionType ActionType { get; set; }

        #endregion

        public event Action<IItem3D> OnFireworkUsed;
        public event Action<float> OnHourglassUsed;

        Sequence _explodeSequence;

        public void InitializeItemAction(InitItemActionContext context)
        {
            Id = context.Id;
            Prefab = context.Prefab;
            PrefabSize = context.PrefabSize;
            ActionType = context.ActionType;

            ObjectRigidbody = Prefab.GetComponent<Rigidbody>();
            ObjectCollider = Prefab.GetComponent<Collider>();
            ObjectOutline = Prefab.GetComponent<ItemOutline>();

            if (ObjectOutline != null)
            {
                ObjectOutline.enabled = false;
            }

            if (ObjectRigidbody != null && ObjectCollider != null)
            {
                Bounds bounds = ObjectCollider.bounds;
                float volume = bounds.size.x * bounds.size.y * bounds.size.z;
                ObjectRigidbody.mass = volume * PrefabSize;
            }
        }

        public void ActionBehaviour(Vector3 targetPos, Action onComplete = null)
        {
            ObjectRigidbody.isKinematic = true;
            ObjectRigidbody.useGravity = false;

            Vector3 middle = new Vector3(0f, 7f, 3.5f);
            Vector3[] path = new[] { middle, targetPos };
            ObjectRigidbody.DOPath(path, 2f, PathType.CatmullRom).OnComplete(() =>
            {
                Destroy(gameObject);
                onComplete?.Invoke();
            });
        }

        public void BlowByFanBooster(float maxX, float maxZ, Action onComplete = null)
        {
            Vector3 startPos = Prefab.transform.position;

            Vector3 nextPos = startPos + Vector3.forward * Random.Range(0, maxLength) +
                              Vector3.up * Random.Range(0, maxHeight);
            nextPos.x = Mathf.Clamp(nextPos.x, -maxX, maxX);
            nextPos.z = Mathf.Clamp(nextPos.z, -maxZ, maxZ);

            Vector3 endPos = nextPos + Vector3.back * Random.Range(0, maxLength);
            endPos.x = Mathf.Clamp(endPos.x, -maxX, maxX);
            endPos.z = Mathf.Clamp(endPos.z, -maxZ, maxZ);

            Vector3[] path = { endPos, nextPos };

            Prefab.transform.DOPath(path, 0.75f, PathType.CatmullRom).OnComplete(() => { onComplete?.Invoke(); });
        }

        public void JumpToBooster(Vector3 targetPos, Action onComplete = null)
        {
        }

        public void JumpFromBooster(Vector3 targetPos, Action onComplete = null)
        {
        }

        public void Explode(Action onComplete = null)
        {
            _explodeSequence = DOTween.Sequence();
            _explodeSequence.Append(Prefab.transform.DOMove(Prefab.transform.position + Vector3.up * 2, 0.25f))
                .Append(Prefab.transform.DOShakePosition(0.25f))
                .Join(Prefab.transform.DOScale(Vector3.zero, 0.25f))
                .OnComplete(() => { Destroy(Prefab.gameObject); });
        }

        #region Item Action Rule

        public void HandleItemAction()
        {
            switch (ActionType)
            {
                case ActionType.Firework:
                    HandleItemActionFirework();
                    break;
                case ActionType.Hourglass:
                    HandleItemActionHourglass();
                    break;
            }
        }

        private void HandleItemActionFirework()
        {
            List<IItem3D> randomItemList = ControllerMatchFactory.Ins.GetListItemRandomByItemAction(maxItemCount);

            if (randomItemList.Count > 0)
            {
                List<ItemAction> fireworkList = new List<ItemAction> { this };
                for (int i = 0; i < randomItemList.Count - 1; i++)
                {
                    ItemAction cloneFirework = Instantiate(this, this.transform.position,
                        this.transform.rotation);
                    InitItemActionContext cloneContext = new InitItemActionContext()
                    {
                        ActionType = this.ActionType,
                        Prefab = cloneFirework.gameObject,
                        PrefabSize = this.PrefabSize,
                    };
                    cloneFirework.InitializeItemAction(cloneContext);
                    fireworkList.Add(cloneFirework);
                }

                for (int i = 0; i < randomItemList.Count; i++)
                {
                    int index = i;
                    fireworkList[i].ActionBehaviour(randomItemList[i].Prefab.transform.position,
                        () => { OnFireworkUsed?.Invoke(randomItemList[index]); });
                }
            }
            else
            {
                Explode();
            }
        }

        private void HandleItemActionHourglass()
        {
            OnHourglassUsed?.Invoke(timeBonus);
            Explode();
        }
    }

    #endregion
}