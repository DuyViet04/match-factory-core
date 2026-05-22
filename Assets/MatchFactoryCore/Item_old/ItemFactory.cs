using System;
using DG.Tweening;
using MatchFactoryCore.Data;
using UnityEngine;

namespace MatchFactoryCore.Item_old
{
    [Serializable]
    public class ItemFactory : MonoBehaviour, IItemFactory3D, IItemFactory2D
    {
        [SerializeField] private ItemFactoryDataSo itemFactoryDataSo;

        // Test
        public Vector3 target;

        // Cache
        bool _is3DObject
        {
            get
            {
                if (Prefab == null || Prefab.activeSelf == false) return false;
                return true;
            }
        }

        private Vector3 _rotation;
        private Vector3 _baseScale;
        private Vector3 _explodeScale;
        private Sequence _explodeSequence;
        private Sequence _jumpSequence;

        void Initialize()
        {
            Debug.Log("Initialize");
            _rotation = Prefab.transform.rotation.eulerAngles;
            _baseScale = Prefab.transform.localScale;
            // _explodeScale = _baseScale + _baseScale * ExplodeScaleBonus;
        }

        #region 3D

        public GameObject Prefab => itemFactoryDataSo.prefab;
        public float Size => itemFactoryDataSo.size;
        public float Weight => itemFactoryDataSo.weight;

        public float ExplodeScaleBonus { get; }
        // public float ExplodeScaleBonus => itemFactoryDataSo.explodeScaleBonus;

        public void JumpFromBoard(Vector3 toTarget)
        {
            Debug.Log("JumpFromBoard");
            _jumpSequence = DOTween.Sequence();
            _jumpSequence.Join(transform.DOJump(toTarget, 2, 1, 0.25f));
            _jumpSequence.Join(transform.DORotate(_rotation, 0.25f));
        }

        public void OnExplode()
        {
            Debug.Log("OnExplode");
            _explodeSequence = DOTween.Sequence();
            _explodeSequence.Append(transform.DOScale(_explodeScale, 0.5f));
            _explodeSequence.Append(transform.DOShakePosition(0.1f));
            _explodeSequence.Append(transform.DOScale(Vector3.zero, 1));
        }

        public void ChangeTo2D()
        {
            Debug.Log("ChangeTo2D");
            Prefab.SetActive(_is3DObject);
            Sprite.SetActive(!_is3DObject);
        }

        #endregion

        #region 2D Behaviour

        public GameObject Sprite => itemFactoryDataSo.sprite;

        public void JumpOnBar()
        {
            throw new NotImplementedException();
        }

        public void Match()
        {
            throw new NotImplementedException();
        }

        public void ChangeTo3D()
        {
            Prefab.SetActive(_is3DObject);
            Sprite.SetActive(!_is3DObject);
        }

        #endregion
    }
}