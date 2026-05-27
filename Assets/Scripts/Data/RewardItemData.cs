using UnityEngine;

namespace Vertigo.Data
{
    public enum RewardType { Currency, Weapon, Consumable, Cosmetic, Points, Chest }

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
        public int Zone;

        public CollectedReward(RewardItemData reward, int amount, int zone)
        {
            Reward = reward;
            Amount = amount;
            Zone = zone;
        }
    }
}
