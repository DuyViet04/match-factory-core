using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game;
using MatchFactoryCore.Scripts.VFX;
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

        [SerializeField] private FireworkBullet fireworkBullet;
        [SerializeField] private ParticleSystem blinkEffect;
        [SerializeField] private ParticleSystem explodeEffect;
        [SerializeField] private ParticleSystem hourglassTrailVFX;
        [SerializeField] private float maxLength = 2;
        [SerializeField] private float maxHeight = 2;
        [SerializeField] private int maxItemCount = 3;
        [SerializeField] private int timeBonus = 10;
        [SerializeField] private int maxHourglassTrail = 3;
        public Rigidbody ObjectRigidbody { get; set; }
        public Collider ObjectCollider { get; set; }
        public ItemOutline ObjectOutline { get; set; }
        public GameObject Prefab { get; set; }
        public float PrefabSize { get; set; }
        public ActionType ActionType { get; set; }
        private Vector3 _baseRotation;

        #endregion

        public event Action<List<IItem3D>> OnFireworkStarted;
        public event Action<IItem3D> OnFireworkBulletMoveCompleted;
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

            _baseRotation = Prefab.transform.rotation.eulerAngles;

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

            //Vector3 middle = new Vector3(0f, 7f, 3.5f);
            //Vector3[] path = new[] { middle, targetPos };
            //ObjectRigidbody.DOPath(path, 2f, PathType.CatmullRom).OnComplete(() =>
            //{
            //    Destroy(gameObject);
            //    onComplete?.Invoke();
            //});
        }

        public void BlowByFanBooster(float maxX, float maxZ, Action onComplete = null)
        {
            if (Prefab == null)
            {
                onComplete?.Invoke();
                return;
            }

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

        public void JumpToBooster(Vector3 targetPos, float delay, Action onComplete = null)
        {
        }

        public void JumpFromBooster(Vector3 targetPos, Action onComplete = null)
        {
        }

        public void Explode(Action onComplete = null)
        {
            _explodeSequence = DOTween.Sequence();
            _explodeSequence.Append(Prefab.transform.DOMove(Prefab.transform.position + Vector3.up * 3, 0.25f))
                .Join(Prefab.transform.DORotate(_baseRotation, 0.25f))
                .Join(Prefab.transform.DOShakePosition(0.25f))
                .Append(Prefab.transform.DOScale(Vector3.zero, 0.1f)
                .OnComplete(() =>
                {
                    SpawnExploreVFX();
                    blinkEffect.Stop();
                    onComplete?.Invoke();
                    Destroy(gameObject);
                }));
        }

        private void SpawnExploreVFX()
        {
            Vector3 spawnPos = Prefab.transform.position;
            spawnPos.y = Camera.main.transform.position.y - 1;
            Quaternion rotation = explodeEffect.transform.rotation;
            var explodeVFX = Instantiate(explodeEffect, spawnPos, rotation);
            explodeVFX.Play();
            StartCoroutine(DestroyVFX(explodeVFX, explodeVFX.main.duration));
        }

        private IEnumerator DestroyVFX(ParticleSystem vfx, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            Destroy(vfx.gameObject);
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
                OnFireworkStarted?.Invoke(randomItemList);
                Explode(() =>
                {
                    ActiveFireworkSkill(randomItemList);
                });
            }
            else
            {
                Explode();
            }
        }

        private void HandleItemActionHourglass()
        {
            OnHourglassUsed?.Invoke(timeBonus);

            Explode(() =>
            {
                Vector3 timeUIPos = ControllerMatchFactory.Ins.GetTimeUIPosition();
                for (int i = 0; i < maxHourglassTrail; i++)
                {
                    Vector3 spawnPos = transform.position;
                    spawnPos.y = timeUIPos.y;
                    var trail = Instantiate(hourglassTrailVFX, spawnPos, Quaternion.identity);
                    HourglassTrailVFX hourglassTrail = trail.GetComponent<HourglassTrailVFX>();
                    hourglassTrail.TargetPos = timeUIPos;
                    hourglassTrail.MoveToTarget(i);
                }
            });
        }

        #endregion

        private void ActiveFireworkSkill(List<IItem3D> targets)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                float delay = i * 0.15f;
                FireworkBullet bullet = Instantiate(fireworkBullet, this.transform.position, this.transform.rotation);
                bullet.Target = targets[i];
                bullet.OnFireworkBulletMoveToTarget += OnFireworkBulletMoveCompleted;
                bullet.MoveToTarget(delay, null, () =>
                {
                    bullet.OnFireworkBulletMoveToTarget -= OnFireworkBulletMoveCompleted;
                });
            }
        }
    }
}