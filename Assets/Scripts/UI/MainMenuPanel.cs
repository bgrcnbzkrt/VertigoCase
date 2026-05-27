using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core;

namespace Vertigo.UI
{
    public class MainMenuPanel : PanelBase
    {
        [SerializeField] private Button btnStart;

        private void Start()
        {
            btnStart.onClick.AddListener(() => GameManager.Instance.StartGame());
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (btnStart == null)
                btnStart = FindButton("ui_btn_start");
        }

        private Button FindButton(string name)
        {
            foreach (var btn in GetComponentsInChildren<Button>(true))
                if (btn.name == name) return btn;
            return null;
        }
    }
}
