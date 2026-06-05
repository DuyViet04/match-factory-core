using DG.Tweening;
using MatchFactoryCore.Scripts.Game;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MatchFactoryCore.Scripts.Item
{
    public class FireworkBullet : MonoBehaviour
    {
        public IItem3D Target { get; set; }

        public void MoveToTarget(float delay)
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

                transform.DOPath(path, 1f, PathType.CatmullRom).SetLookAt(0.1f)
                    .SetDelay(delay)
                    .OnComplete(() =>
                {
                    Target.Explode();
                    Destroy(gameObject);
                });
            }
        }
    }
}
