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
            reviveCostText.text = cost.ToString();
            goldText.text = CurrencyManager.Instance.Gold.ToString();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            foreach (var btn in GetComponentsInChildren<Button>(true))
            {
                if (btn.name == "ui_button_give_up") buttonGiveUp = btn;
                if (btn.name == "ui_button_revive") buttonRevive = btn;
            }
        }
    }
}
