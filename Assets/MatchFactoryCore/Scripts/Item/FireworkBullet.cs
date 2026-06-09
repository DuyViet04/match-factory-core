using DG.Tweening;
using MatchFactoryCore.Scripts.Game;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MatchFactoryCore.Scripts.Item
{
    public class FireworkBullet : MonoBehaviour
    {
        public IItem3D Target { get; set; }
        public event Action<IItem3D> OnFireworkBulletMoveToTarget;

        public void MoveToTarget(float delay, Action onComplete = null, Action onKill = null)
        {
            if (Target != null)
            {
                Vector3 anchorPos = ControllerMatchFactory.Ins.FireworkAnchorPos;
                float randX = Random.Range(-1f, 1f);
                float randZ = Random.Range(-1f, 1f);
                anchorPos.x += randX;
                anchorPos.z += randZ;
                Vector3 endPos = Target.Prefab.transform.position;
                Vector3[] path = { anchorPos, endPos };

                transform.DOPath(path, 1f, PathType.CatmullRom)
                    .SetLookAt(0.1f, Vector3.up)
                    .SetDelay(delay)
                    .OnComplete(() =>
                    {
                        OnFireworkBulletMoveToTarget?.Invoke(Target);
                        onComplete?.Invoke();
                    })
                    .OnKill(() =>
                    {
                        onKill?.Invoke();
                        Destroy(gameObject);
                    });
            }
        }
    }
}
