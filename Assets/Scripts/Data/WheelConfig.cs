using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Data
{
    [CreateAssetMenu(fileName = "NewWheelConfig", menuName = "Vertigo/Wheel Config")]
    public class WheelConfig : ScriptableObject
    {
        public Sprite baseSprite;
        public Sprite indicatorSprite;
        public List<SliceData> slices = new();

        [Header("Spin Settings")]
        public float spinDuration = 3f;
        public int minRotations = 3;
        public int maxRotations = 6;

        [Header("Reward Scaling")]
        public float rewardGrowthPerZone = 0.15f;

        public int ScaleAmount(int baseAmount, int zone)
        {
            // linear scale, zone 1 = base
            return Mathf.RoundToInt(baseAmount * (1f + rewardGrowthPerZone * (zone - 1)));
        }
    }

    [Serializable]
    public class SliceData
    {
        public RewardItemData reward;
        public int amount = 1;
    }
}
