using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Data
{
    public enum WheelType { Bronze, Silver, Golden }

    [CreateAssetMenu(fileName = "NewWheelConfig", menuName = "Vertigo/Wheel Config")]
    public class WheelConfig : ScriptableObject
    {
        public WheelType type;
        public Sprite baseSprite;
        public Sprite indicatorSprite;
        public List<SliceData> slices = new List<SliceData>();

        [Header("Spin Settings")]
        public float spinDuration = 3f;
        public int minRotations = 3;
        public int maxRotations = 6;
    }

    [Serializable]
    public class SliceData
    {
        public RewardItemData reward;
        public int amount = 1;
        public bool isBomb;
    }
}
