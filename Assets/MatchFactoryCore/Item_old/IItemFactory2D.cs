using UnityEngine;

namespace MatchFactoryCore.Item_old
{
    public interface IItemFactory2D
    {
        public GameObject Sprite { get; }

        public void JumpOnBar();
        public void Match();
        public void ChangeTo3D();
    }
}