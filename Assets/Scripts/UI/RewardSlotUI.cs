using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Data;

namespace Vertigo.UI
{
    public class RewardSlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;

        public void Setup(CollectedReward reward)
        {
            icon.sprite = reward.Reward.icon;
            amountText.text = "x" + reward.Amount;
        }

        private void OnValidate()
        {
            if (icon == null) icon = GetComponentInChildren<Image>();
            if (amountText == null) amountText = GetComponentInChildren<TMP_Text>();
        }
    }
}
