using UnityEngine;
using UnityEngine.UI;

namespace MatchFactoryCore.Scripts.Item
{
    public enum JumpTypeMatch
    {
        Left,
        Center,
        Right,
    }
    
    public class ItemFactory2D : MonoBehaviour
    {
        public int Id { get; private set; }
        public IItemFactory2D ItemFactory { get; private set; }
        [SerializeField] private Image image;

        public void Initialize(int id, IItemFactory2D itemFactory)
        {
            Id = id;
            ItemFactory = itemFactory;
            image.sprite = ItemFactory.Sprite;
        }

        public void JumpOnBar(Vector3 position)
        {
            
        }

        public void JumpMatch(Vector3 position, JumpTypeMatch jumpType)
        {
            switch (jumpType)
            {
                case JumpTypeMatch.Left:
                    
                    break;
                case JumpTypeMatch.Center:
                    break;
                case JumpTypeMatch.Right:
                    break;
            }
        }
    }
}