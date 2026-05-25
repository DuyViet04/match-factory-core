using MatchFactoryCore.Scripts.Data;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Item
{
    public abstract class ItemEntity : MonoBehaviour
    {
        public int Id { get; private set; }
        public ItemFactoryType FactoryType { get; private set; }
    }
}