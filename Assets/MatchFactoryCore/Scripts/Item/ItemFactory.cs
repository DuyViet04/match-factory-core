using DG.Tweening;
using MatchFactoryCore.Scripts.Data;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public class ItemFactory : MonoBehaviour, IItemFactory3D
    {
        private Rigidbody _rigidbody;
        private Collider _collider;

        private Vector3 _rotation;
        private Sequence _jumpSequence;

        public int Id { get; set; }
        public ItemType Type { get; set; }
        public GameObject Sprite { get; set; }

        public void Initialize(int id, ItemType type, GameObject prefab, GameObject sprite, float size)
        {
            Id = id;
            Type = type;
            Prefab = prefab;
            Sprite = sprite;
            Size = size;

            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            if (_rigidbody != null && _collider != null)
            {
                var b = _collider.bounds;
                var volume = b.size.x * b.size.y * b.size.z;
                _rigidbody.mass = volume * size;
                Weight = _rigidbody.mass;
            }

            _rotation = Prefab.transform.rotation.eulerAngles;
            // Debug.Log($"{id}: {Prefab.name} {Sprite.name} {Size} {Weight}");
        }

        #region 3D Object

        public GameObject Prefab { get; set; }
        public float Size { get; set; }
        public float Weight { get; set; }

        public void JumpFromBoard(Vector3 toTarget)
        {
            Debug.Log("JumpFromBoard");
            _jumpSequence = DOTween.Sequence();
            _jumpSequence.Join(transform.DOJump(toTarget, 1, 1, 0.25f));
            _jumpSequence.Join(transform.DORotate(_rotation, 0.25f));
        }

        public void OnExplode()
        {
        }

        public void ChangeTo2D(out GameObject sprite)
        {
            sprite = null;
            Debug.Log("ChangeTo2D");
            Prefab.gameObject.SetActive(false);
            sprite = Instantiate(Sprite, Prefab.transform.position, Sprite.transform.rotation);
            sprite.AddComponent<ItemSprite>();
            sprite.SetActive(true);
        }

        #endregion
    }
}