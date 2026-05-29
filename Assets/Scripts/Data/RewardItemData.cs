using UnityEngine;

namespace Vertigo.Data
{
    public enum RewardType { Gold, Weapon, Consumable, Cosmetic, Points, Chest, Money, Bomb }

    [CreateAssetMenu(fileName = "NewReward", menuName = "Vertigo/Reward Item")]
    public class RewardItemData : ScriptableObject
    {
        public string rewardName;
        public Sprite icon;
        public RewardType type;
        public int baseAmount = 1;
    }

    public struct CollectedReward
    {
        public RewardItemData Reward;
        public int Amount;

        public CollectedReward(RewardItemData reward, int amount)
        {
            Reward = reward;
            Amount = amount;
        }
    }
}
