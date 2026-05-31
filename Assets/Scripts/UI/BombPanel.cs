using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core;

namespace Vertigo.UI
{
    public class BombPanel : PanelBase
    {
        [SerializeField] private Button buttonGiveUp;
        [SerializeField] private Button buttonRevive;
        [SerializeField] private TMP_Text reviveCostText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text moneyText;

        private void Start()
        {
            buttonGiveUp.onClick.AddListener(() => GameManager.Instance.GiveUp());
            buttonRevive.onClick.AddListener(() => GameManager.Instance.Revive());
        }

        public override void Show()
        {
            base.Show();
            int cost = CurrencyManager.Instance.ReviveCost;
            bool canAfford = CurrencyManager.Instance.CanAfford(cost);
            buttonRevive.interactable = canAfford;
            reviveCostText.SetText("{0}", cost);
            goldText.SetText("{0}", CurrencyManager.Instance.Gold);
            moneyText.SetText("{0}", CurrencyManager.Instance.Money);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            buttonGiveUp = FindButton("ui_button_give_up");
            buttonRevive = FindButton("ui_button_revive");
        }
    }
}
