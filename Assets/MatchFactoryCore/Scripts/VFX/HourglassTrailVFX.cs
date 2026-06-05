using DG.Tweening;
using UnityEngine;

namespace MatchFactoryCore.Scripts.VFX
{
    public class HourglassTrailVFX : MonoBehaviour
    {
        public Vector3 TargetPos { get; set; }

        public void MoveToTarget(int index)
        {
            Vector3 midPos = transform.position;
            switch (index)
            {
                case 0:
                    midPos += Vector3.left * Random.Range(0.5f, 1f)
                        + Vector3.forward * Random.Range(0.5f, 1f);
                    break;
                case 1:
                    midPos += Vector3.right * Random.Range(0.5f, 1f)
                        + Vector3.forward * Random.Range(0.5f, 1f);
                    break;
                case 2:
                    midPos += Vector3.back * Random.Range(0.5f, 1f)
                        + Vector3.left * Random.Range(-0.5f, 0.5f);
                    break;
            }

            Vector3[] path = { midPos, TargetPos };
            transform.DOPath(path, 0.75f, PathType.CatmullRom)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
