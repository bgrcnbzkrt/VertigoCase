using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core;

namespace Vertigo.UI
{
    public class BombPanel : PanelBase
    {
        [SerializeField] private Button btnGiveUp;
        [SerializeField] private Button btnRevive;

        private void Start()
        {
            btnGiveUp.onClick.AddListener(() => GameManager.Instance.GiveUp());
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
