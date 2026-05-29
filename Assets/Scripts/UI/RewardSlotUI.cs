using DG.Tweening;
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

        public RewardItemData RewardData { get; private set; }
        private int totalAmount;

        public void Setup(CollectedReward reward)
        {
            RewardData = reward.Reward;
            totalAmount = reward.Amount;
            icon.sprite = reward.Reward.icon;
            icon.preserveAspect = true;
            amountText.text = "x" + totalAmount;
        }

        public void AddAmount(int amount)
        {
            totalAmount += amount;
            amountText.text = "x" + totalAmount;
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0.5f);
        }

        private void OnValidate()
        {
            if (icon == null) icon = GetComponentInChildren<Image>();
            if (amountText == null) amountText = GetComponentInChildren<TMP_Text>();
        }
    }
}
