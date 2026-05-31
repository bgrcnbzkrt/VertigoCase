using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core;

namespace Vertigo.UI
{
    public class MainMenuPanel : PanelBase
    {
        [SerializeField] private Button buttonStart;

        private void Start()
        {
            buttonStart.onClick.AddListener(() => GameManager.Instance.StartGame());
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            buttonStart = FindButton("ui_button_start");
        }
    }
}
