using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core;

namespace Vertigo.UI
{
    public class BombPanel : PanelBase
    {
        [SerializeField] private Button btnGiveUp;
        [SerializeField] private Button btnRevive;
        [SerializeField] private TMP_Text reviveCostText;

        private void Start()
        {
            btnGiveUp.onClick.AddListener(() => GameManager.Instance.GiveUp());
            btnRevive.onClick.AddListener(() => GameManager.Instance.Revive());
        }

        public override void Show()
        {
            base.Show();
            int cost = CurrencyManager.Instance.ReviveCost;
            bool canAfford = CurrencyManager.Instance.CanAfford(cost);
            btnRevive.interactable = canAfford;
            if (reviveCostText != null)
                reviveCostText.text = cost.ToString();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            foreach (var btn in GetComponentsInChildren<Button>(true))
            {
                if (btn.name == "ui_btn_give_up") btnGiveUp = btn;
                if (btn.name == "ui_btn_revive") btnRevive = btn;
            }
        }
    }
}
