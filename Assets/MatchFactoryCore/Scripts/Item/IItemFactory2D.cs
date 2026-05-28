using MatchFactoryCore.Scripts.Data;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public interface IItemFactory2D
    {
        public Sprite Sprite { get; set; }

        public ItemFactoryType ItemFactoryType { get; set; }
        public float SpriteScaleOnBar { get; set; }
        public float SpriteScaleWhenChange { get; set; }

        public void ChangeTo3D();
    }
}