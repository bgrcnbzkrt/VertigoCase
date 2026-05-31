using System.Collections.Generic;
using Vertigo.Data;

namespace Vertigo.Core
{
    // Holds the rewards collected during a single run.
    public class RewardCollection
    {
        private readonly List<CollectedReward> items = new();

        public IReadOnlyList<CollectedReward> Items => items;

        // adds the amount, stacking onto an existing row if we already won this reward
        public void Add(RewardItemData reward, int amount)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Reward == reward)
                {
                    var existing = items[i];
                    existing.Amount += amount;
                    items[i] = existing;
                    return;
                }
            }
            items.Add(new CollectedReward(reward, amount));
        }

        public void Clear() => items.Clear();
    }
}
