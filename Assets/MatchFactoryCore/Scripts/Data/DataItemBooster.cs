using System;
using MatchFactoryCore.Scripts.Item;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Data
{
    [Serializable]
    public class DataItemBooster
    {
        public Sprite boosterSprite;
        public BoosterType boosterType;
        public int boosterCount;

        public DataItemBooster()
        {
        }
        
        public DataItemBooster(DataItemBooster source)
        {
            boosterSprite = source.boosterSprite;
            boosterType = source.boosterType;
            boosterCount = source.boosterCount;
        }
    }
}